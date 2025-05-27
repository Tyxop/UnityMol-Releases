/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace UMol
{

    enum XDRFileReaderStatus
    {
        OFFSETFILECREATION = -7,
        OFFSETFILETREAD = -6,
        TRAJECTORYPRESENT = -5,
        FRAMEDOESNOTEXIST = -4,
        ENDOFFILE = -3,
        NUMBEROFATOMSMISMATCH = -2,
        FILENOTFOUND = -1,
        SUCCESS = 0
    }

    /// <summary>
    /// Native C# implementation of XDR file reader without DLL dependencies
    /// </summary>
    public class XDRFileReader
    {
        private int TRAJBUFFERSIZE = 40;

        public UnityMolStructure structure;
        public int currentFrame = 0;
        public int numberAtoms = 0;
        public int numberFrames = 0;

        // Native file handling instead of IntPtr
        private string trajectoryFilePath;
        private bool isFileOpen = false;
        public string offsetFileName;
        public long[] offsets;
        bool is_trr = false;

        float[,] box = new float[3, 3];

        public struct FrameInfo
        {
            public int step;
            public float time;
        }

        List<FrameInfo> frames_info;

        /// Buffer of frames containing TRAJBUFFERSIZE/2 frames before the current frame and TRAJBUFFERSIZE/2 after
        private Vector3[][] trajectoryBuffer;
        /// Get position of the frame in the buffer
        private Dictionary<int, int> frameToTrajBuffer = new Dictionary<int, int>();
        int idB = 0;
        private float[] trajectoryBufferF;
        private TrajectorySmoother trajSmoother;

        /// Initiate at frame 0 and returns the number of frames
        public int load_trajectory()
        {
            trajSmoother = new TrajectorySmoother();
            sync_scene_with_frame(0);
            return numberFrames;
        }

        /// Opens a trajectory file using native C# implementation
        public int open_trajectory(UnityMolStructure stru, string filename, bool is_trr = false)
        {
            if (isFileOpen)
            {
                Debug.LogError("This instance has a trajectory already opened.");
                return (int)XDRFileReaderStatus.TRAJECTORYPRESENT;
            }
            if (stru.trajectoryLoaded)
            {
                Debug.LogError("This structure has a trajectory already opened.");
                return (int)XDRFileReaderStatus.TRAJECTORYPRESENT;
            }

            structure = stru;
            trajectoryFilePath = filename;

            // Check if file exists
            if (!File.Exists(filename))
            {
                Debug.LogError("Could not open file " + filename);
                return (int)XDRFileReaderStatus.FILENOTFOUND;
            }

            try
            {
                if (is_trr)
                {
                    // TRR support would need additional implementation
                    Debug.LogError("TRR format not supported in native implementation yet");
                    return (int)XDRFileReaderStatus.FILENOTFOUND;
                }
                else
                {
                    // Use XTCTrajectoryParserCSharp for XTC files
                    numberAtoms = XTCTrajectoryParserCSharp.GetAtomCount(filename);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not get number of atoms from file " + filename + ": " + e.Message);
                return (int)XDRFileReaderStatus.FILENOTFOUND;
            }

            if (numberAtoms > structure.Count)
            {
                Debug.LogWarning("Trajectory has not the same number of atoms than the first model of the structure." + numberAtoms + " vs " + structure.Count);
            }

            isFileOpen = true;

            int res = updateOffsetFile(filename);
            if (res != (int)XDRFileReaderStatus.SUCCESS)
            {
                if (res == (int)XDRFileReaderStatus.OFFSETFILETREAD)
                {
                    Debug.LogError("Could not read offset file " + offsetFileName);
                    return (int)XDRFileReaderStatus.OFFSETFILETREAD;
                }

                Debug.LogError("Could not create offset file " + offsetFileName);
                return (int)XDRFileReaderStatus.OFFSETFILECREATION;
            }

            this.is_trr = is_trr;
            structure.trajectoryLoaded = true;
            return numberAtoms;
        }

        bool diffFrames(Vector3[] f1, Vector3[] f2)
        {
            if (f1.Length != f2.Length)
            {
                return false;
            }
            for (int i = 0; i < f1.Length; i++)
            {
                if (!Mathf.Approximately(f1[i].x, f2[i].x) ||
                    !Mathf.Approximately(f1[i].y, f2[i].y) ||
                    !Mathf.Approximately(f1[i].z, f2[i].z))
                {
                    return false;
                }
            }
            return true;
        }

        public Vector3[] getFrame(int frame_number)
        {
            if (trajectoryBuffer == null || trajectoryBuffer[0] == null)
            {
                if (numberFrames < TRAJBUFFERSIZE)
                {
                    TRAJBUFFERSIZE = numberFrames;
                }
                trajectoryBuffer = new Vector3[TRAJBUFFERSIZE][];
                for (int i = 0; i < TRAJBUFFERSIZE; i++)
                {
                    trajectoryBuffer[i] = new Vector3[numberAtoms];
                }
                trajectoryBufferF = new float[numberAtoms * 3];
            }

            if (frameToTrajBuffer.ContainsKey(frame_number))
            { //Already in memory
                return trajectoryBuffer[frameToTrajBuffer[frame_number]];
            }

            //Not in memory => load TRAJBUFFERSIZE/4 before and after = fills half of the buffer
            loadBufferFrames(frame_number);

            return trajectoryBuffer[frameToTrajBuffer[frame_number]];
        }

        /// Load TRAJBUFFERSIZE/4 frames before and after frame_number
        private void loadBufferFrames(int frame_number)
        {
            int i = 0;

            int startF = frame_number - TRAJBUFFERSIZE / 4;
            while (i < TRAJBUFFERSIZE / 2)
            {//While not filled half of the array
                if (startF + i < 0)
                {
                    startF++;
                    continue;
                }
                if (startF + i >= numberFrames)
                {
                    startF = -i;
                    continue;
                }
                int idF = startF + i;
                loadOneFrame(idF);
                i++;
            }
        }

        /// Load one frame in the buffer using native C# XTC parser
        private Vector3[] loadOneFrame(int frame_number)
        {
            try
            {
                // Get single frame using XTCTrajectoryParserCSharp
                List<Vector3[]> singleFrame = XTCTrajectoryParserCSharp.GetTrajectory(trajectoryFilePath, frame_number, 1, 1);

                if (singleFrame.Count == 0)
                {
                    Debug.LogError($"Could not read frame {frame_number}");
                    return null;
                }

                Vector3[] frameData = singleFrame[0];

                // Copy to buffer with coordinate transformation (already done in XTCTrajectoryParserCSharp)
                for (int i = 0; i < Mathf.Min(numberAtoms, frameData.Length); i++)
                {
                    trajectoryBuffer[idB][i] = frameData[i];
                }

                //Remove previous value
                int fToDel = -1;
                foreach (var f in frameToTrajBuffer)
                {
                    if (f.Value == idB)
                    {
                        fToDel = f.Key;
                    }
                }
                if (fToDel != -1)
                    frameToTrajBuffer.Remove(fToDel);

                Vector3[] frame = trajectoryBuffer[idB];
                frameToTrajBuffer[frame_number] = idB;

                idB++;
                if (idB == TRAJBUFFERSIZE)
                {
                    idB = 0;
                }
                return frame;

            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading frame {frame_number}: {e.Message}");
                return null;
            }
        }

        public int updateOffsetFile(string trajFile, bool forceCreate = false)
        {
            offsetFileName = trajFile + ".offset";
            bool offsetExists = File.Exists(offsetFileName);

            if (forceCreate)
            {
                return createOffsetFile(trajFile);
            }
            if (offsetExists)
            {
                DateTime lastModif = File.GetLastWriteTime(offsetFileName);
                DateTime creationTraj = File.GetLastWriteTime(trajFile);
                if (lastModif > creationTraj)
                { //Offset file is posterior to creation of traj file
                    try
                    {
                        return readOffsetFile();
                    }
                    catch
                    {//Failed to read offset file => create one
                    }
                }
            }

            return createOffsetFile(trajFile);
        }

        int createOffsetFile(string fileName)
        {
            try
            {
                if (!is_trr)
                {//XTC
                 // Use native C# implementation to get frame count
                    numberFrames = XTCTrajectoryParserCSharp.GetFrameCount(fileName);

                    if (numberFrames <= 0)
                    {
                        Debug.LogError("Could not determine number of frames");
                        return (int)XDRFileReaderStatus.OFFSETFILECREATION;
                    }

                    // Create dummy offsets - XTCTrajectoryParserCSharp handles frame seeking internally
                    offsets = new long[numberFrames];
                    for (int i = 0; i < numberFrames; i++)
                    {
                        offsets[i] = i; // Simple frame index as offset
                    }
                }
                else
                {
                    Debug.LogError("TRR format not supported in native implementation yet");
                    return (int)XDRFileReaderStatus.OFFSETFILECREATION;
                }

                BinaryWriter bw = new BinaryWriter(new FileStream(offsetFileName, FileMode.Create));
                bw.Write((Int32)numberFrames);
                for (int i = 0; i < numberFrames; i++)
                {
                    bw.Write(offsets[i]);
                }
                bw.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return (int)XDRFileReaderStatus.OFFSETFILECREATION;
            }

            return (int)XDRFileReaderStatus.SUCCESS;
        }

        int readOffsetFile()
        {
            try
            {
                BinaryReader br = new BinaryReader(new FileStream(offsetFileName, FileMode.Open));
                numberFrames = br.ReadInt32();
                if (numberFrames > 0 && numberFrames < 2e9)
                { //2 billion seems like a fair upper limit for trajectories
                    offsets = new long[numberFrames];
                    for (int i = 0; i < numberFrames; i++)
                    {
                        offsets[i] = br.ReadInt64();
                    }
                }
                else
                {
                    br.Close();
                    return (int)XDRFileReaderStatus.OFFSETFILETREAD;
                }
                br.Close();
            }
            catch
            {
                return (int)XDRFileReaderStatus.OFFSETFILETREAD;
            }

            return (int)XDRFileReaderStatus.SUCCESS;
        }

        /// Native implementation of next_frame using XTCTrajectoryParserCSharp
        public int next_frame(ref int step, ref float time, float[] positions, ref float precision)
        {
            if (!isFileOpen)
            {
                Debug.LogWarning("Trajectory was not previously opened.");
                return (int)XDRFileReaderStatus.FILENOTFOUND;
            }

            try
            {
                // This is a simplified implementation - you might need to track current position
                // For now, we'll use the frame loading mechanism
                Vector3[] frame = getFrame(currentFrame);
                if (frame != null)
                {
                    for (int i = 0; i < Mathf.Min(numberAtoms, frame.Length); i++)
                    {
                        positions[i * 3] = -frame[i].x / 10.0f;     // Convert back from display units
                        positions[i * 3 + 1] = frame[i].y / 10.0f;
                        positions[i * 3 + 2] = frame[i].z / 10.0f;
                    }
                    step = currentFrame;
                    time = currentFrame * 1.0f; // Placeholder time
                    precision = 1000.0f; // Placeholder precision
                    return (int)XDRFileReaderStatus.SUCCESS;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in next_frame: " + e.Message);
            }

            return (int)XDRFileReaderStatus.ENDOFFILE;
        }

        public int sync_scene_with_frame(int frame_number)
        {
            if (frame_number >= numberFrames)
            {
                Debug.LogError("Frame number " + frame_number + " does not exist.");
                return (int)XDRFileReaderStatus.FRAMEDOESNOTEXIST;
            }

            Vector3[] f = getFrame(frame_number);
            if (f == null)
            {
                return (int)XDRFileReaderStatus.FRAMEDOESNOTEXIST;
            }

            structure.trajAtomPositions = f;
            structure.trajUpdateAtomPositions();

            currentFrame = frame_number;
            return (int)XDRFileReaderStatus.SUCCESS;
        }

        public int close_trajectory()
        {
            if (trajSmoother != null)
            {
                trajSmoother.clear();
            }

            if (!isFileOpen)
            {
                return (int)XDRFileReaderStatus.FILENOTFOUND;
            }

            offsets = null;
            isFileOpen = false;
            trajectoryFilePath = null;
            numberAtoms = 0;
            numberFrames = 0;
            frameToTrajBuffer.Clear();
            structure.trajectoryLoaded = false;

            return (int)XDRFileReaderStatus.SUCCESS;
        }

        public void Clear()
        {
            close_trajectory();
            if (structure.trajAtomPositions != null)
            {
                structure.trajAtomPositions = null;
            }
        }

        public int sync_scene_with_frame_smooth(int frame1, int frame2, float t, bool new_frame = false)
        {
            if (frame1 >= numberFrames || frame1 < 0)
            {
                Debug.LogError("Frame number " + frame1 + " does not exist.");
                return (int)XDRFileReaderStatus.FRAMEDOESNOTEXIST;
            }
            if (frame2 >= numberFrames || frame2 < 0)
            {
                Debug.LogError("Frame number " + frame2 + " does not exist.");
                return (int)XDRFileReaderStatus.FRAMEDOESNOTEXIST;
            }

            t = Mathf.Clamp(t, 0.0f, 1.0f);

            Vector3[] f1 = getFrame(frame1);
            Vector3[] f2 = getFrame(frame2);

            if (f1 == null || f2 == null)
            {
                return (int)XDRFileReaderStatus.FRAMEDOESNOTEXIST;
            }

            trajSmoother.init(f1, f2);
            trajSmoother.process(structure.trajAtomPositions, t);

            structure.trajUpdateAtomPositions();

            //Not always updating currentFrame
            if (new_frame)
                currentFrame = frame1;

            return (int)XDRFileReaderStatus.SUCCESS;
        }
    }

} // namespace UMol