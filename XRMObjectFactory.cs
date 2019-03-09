using UnityEngine;
using GoogleARCore;

namespace XRM
{
    public class XRMObjectFactory : MonoBehaviour
    {
        public XRMObjectLibrary objectLibrary;
        public GameObject sceneRoot;
        private XRMObject selectedObject;
        public XRMObject SelectedObject
        {
            get
            {
                return selectedObject;
            }
        }

        private void Awake()
        {
            // TEMP - selected object is the first in the list
            SetSelectedObject(objectLibrary.objects[0]);
        }

        public void OnHitTestSuccess(ARHitData hit)
        {
            if (sceneRoot == null)
            {
                Debug.LogWarning("SceneRoot not set");
                return;
            }

            if (selectedObject.InstanceCount < selectedObject.maxInstances)
            {
                CreateInstance(hit.position);
                selectedObject.IncrementInstanceCount();
            }
        }

        public void OnHitTestSuccess(TrackableHit hit)
        {
            if (sceneRoot == null)
            {
                Debug.LogWarning("SceneRoot not set");
                return;
            }

            if (selectedObject.InstanceCount < selectedObject.maxInstances)
            {
                CreateInstance(hit.Pose.position);
                selectedObject.IncrementInstanceCount();
            }
        }

        private void CreateInstance(Vector3 position)
        {
            GameObject instance = Instantiate(selectedObject.modelPrefab);
            instance.transform.localPosition = position;
            instance.transform.parent = sceneRoot.transform;
        }

        public void SetSelectedObject(XRMObject xrmObject)
        {
            selectedObject = xrmObject;
        }

        public void ResetInstanceCounts()
        {
            foreach (XRMObject xrmObject in objectLibrary.objects)
            {
                xrmObject.InstanceCount = 0;
            }
        }
    }
}