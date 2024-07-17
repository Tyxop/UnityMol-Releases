



namespace UMol
{

    using UnityEngine;

    



    public static class VR_data {

        public static Transform LeftController;
        public static Transform RightController;

        public static Transform HeadSet;

        public static Transform GetHeadSet() {

            return HeadSet;
        }
    }

}

namespace VRGlue {
    using UnityEngine;


    public struct InteractableObjectEventArgs
    {
        public GameObject interactingObject;
    }


}