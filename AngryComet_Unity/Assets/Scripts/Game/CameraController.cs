using System.Collections;
using UnityEngine;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        //Singleton object
        public static CameraController Instance = null;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                //DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }
    
        public Transform target;
        public float dampTime = 0.125f;
        public Vector3 offset;
    
        [Space]

        private Camera _camera;
        private Vector3 _velocity = Vector3.zero;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target)
            {   
                //follow the player position smoothly (with smooth damp)
                var tarPos = target.position;
                var position = transform.parent.position;
            
                Vector3 point = _camera.WorldToViewportPoint(tarPos);
                Vector3 delta = tarPos - _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
                Vector3 destination = transform.position + offset + delta;
                transform.parent.position = Vector3.SmoothDamp(position, destination, ref _velocity, dampTime);
            }
        }

        public IEnumerator Shake(float duration, float magnitude)
        {
            Vector3 originalPos = transform.localPosition;

            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                //change camera position to random position within range every frame for <duration> seconds
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
            
                transform.localPosition = new Vector3(x, y, originalPos.z);
                elapsed += Time.deltaTime;
            
                yield return null;
            }
            
            //reset camera position to original position before shake
            transform.localPosition = originalPos;
        }
    }
}
