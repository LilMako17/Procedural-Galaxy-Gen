using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CubeSphere
{
    public class PlanetCursor : MonoBehaviour
    {
        public Vector3 CurrentPos;

        public float Speed = 1f;
        public float WASD_Speed = 1f;
        public float ProjectionWidth = 1f;
        public float ProjectionHeight = 1f;
        public float CameraDistance = 10;
        public bool EnableMouseMove = false;

        public NoisePreview NoisePreview;
        public Planet Planet;

        public Camera MainCamera;
        public GameObject Cursor;
        public GameObject LocalNormalDebug;

        // UI
        public TMP_Text DebugText;
        public Button TeleportButton;
        public TMP_InputField XInput;
        public TMP_InputField YInput;

        // debug objects show the boundss
        public GameObject[] Corners;

        // camera for projecting frustrum, does not render
        public Camera Camera;

        private Vector3[] _viewableXY;
        private Vector3[] _viewPort;
        private Plane _groundPlane;
        private Vector3 _lastMousePos;
        private List<Vector3> _debugPoints = new List<Vector3>();
        private float _lookRotation = 0f;

        private bool _init = false;

        // Start is called before the first frame update
        void Start()
        {
            // DEBUG
            var r = Planet.ShapeSettings.Radius;
            _debugPoints.Clear();
            for (int i = -90; i < 90; i += 10)
            {
                _debugPoints.Add(Planet.transform.TransformPoint(LatLonTo3dSpace(new Vector2(i, 0), r)));
                _debugPoints.Add(Planet.transform.TransformPoint(LatLonTo3dSpace(new Vector2(i, 180), r)));
            }

            for (int i = -180; i < 180; i += 10)
            {
                _debugPoints.Add(Planet.transform.TransformPoint(LatLonTo3dSpace(new Vector2(0, i), r)));
            }

            if (Camera == null)
            {
                Camera = GetComponent<Camera>();
            }
            _viewPort = new Vector3[4];
            _viewableXY = new Vector3[4];
            _viewPort[0] = new Vector3(0, 0, Camera.farClipPlane);
            _viewPort[1] = new Vector3(1, 0, Camera.farClipPlane);
            _viewPort[2] = new Vector3(1, 1, Camera.farClipPlane);
            _viewPort[3] = new Vector3(0, 1, Camera.farClipPlane);

            // UI
            if (TeleportButton)
            {
                TeleportButton.onClick.AddListener(OnTeleportButtonPressed);
            }
        }

        void Update()
        {

            if (!_init)
            {
                TeleportToLatLonCoords(0, -90f);
                _init = true; 
            }

            var keyboard = false;
            Vector3 inputVector = new Vector3(0, 0, 0);
            

            if (Input.GetKey(KeyCode.W))
            {
                inputVector.x += 1f;
                keyboard = true;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputVector.z -= 1f;
                keyboard = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputVector.x -= 1f;
                keyboard = true;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputVector.z += 1f;
                keyboard = true;
            }
            if (Input.GetKey(KeyCode.E))
            {
                inputVector.y += 1f;
                _lookRotation -= 1f;
                _lookRotation %= 360f;
                keyboard = true;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                inputVector.y -= 1f;
                _lookRotation += 1f;
                _lookRotation %= 360f;
                keyboard = true;
            }

            // dont do mouse input if keyboard
            if (keyboard)
            {
                Cursor.transform.rotation *= Quaternion.Euler(inputVector * WASD_Speed);
                var localpos = Cursor.transform.position;

                /*var xAxis = GetXAxis(CurrentPos);
                var yAxis = GetYAxis(CurrentPos);
                var xAngle = Quaternion.Euler(xAxis.normalized * inputVector.x); //Quaternion.AngleAxis(inputVector.x, xAxis.normalized);
                //Debug.Log(xAngle + " vs "+xAxis);
                var yAngle = Quaternion.Euler(yAxis.normalized * inputVector.y); //Quaternion.AngleAxis(inputVector.y, yAxis.normalized);
                var localPos = yAngle * xAngle * CurrentPos;


                //Planet.transform.Rotate(xAxis, inputVector.x * WASD_Speed);
                //Planet.transform.Rotate(yAxis, inputVector.y * WASD_Speed);
                //var camVector = new Vector3(inputVector.x, inputVector.y * -1, 0);
                inputVector.y *= -1;
                var camVector = yAngle * xAngle * inputVector;
                //MainCamera.transform.RotateAround(Planet.transform.position, camVector, 1f);

                Planet.transform.TransformPoint(localPos)
                */


                UpdatePositionCoords(localpos);
                return;
            }

            var mousePos = Input.mousePosition;
            if (_lastMousePos == mousePos)
            {
                return;
            }

            _lastMousePos = mousePos;
            var ray = MainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit) && hit.transform.gameObject == Planet.gameObject)
            {

                if (Input.GetMouseButton(0))
                {
                    var delta = Speed * -Time.deltaTime;
                    Planet.transform.Rotate(-Input.GetAxis("Mouse Y") * delta, (Input.GetAxis("Mouse X") * delta), 0, Space.World);
                    //var camVector = new Vector3(Input.GetAxis("Mouse Y"), -1 * Input.GetAxis("Mouse X") , 0f);
                    //MainCamera.transform.RotateAround(Planet.transform.position, camVector, delta);
                    UpdatePositionCoords(Planet.transform.TransformPoint(CurrentPos));
                    return;
                }

                if (EnableMouseMove)
                {
                    UpdatePositionCoords(hit.point);
                }
            }
        }

        private void OnTeleportButtonPressed()
        {
            var lat = 0f;
            var lon = 0f;
            if (XInput && YInput)
            {
                lat = float.Parse(XInput.text);
                lon = float.Parse(YInput.text);
            }

            TeleportToLatLonCoords(lat, lon);
        }

        private void TeleportToLatLonCoords(float lat, float lon)
        {
            var localPoint = LatLonTo3dSpace(new Vector2(lat, lon), Planet.ShapeSettings.Radius);
            UpdatePositionCoords(Planet.transform.TransformPoint(localPoint));
            Debug.Log(Cursor.transform.localRotation);
        }

        private void OnDrawGizmos()
        {
            foreach (var point in _debugPoints)
            {
                Gizmos.DrawSphere(point, 5);
            }
        }

        private void SetCursorPositionAndRotation(Vector3 worldPos)
        {
            var localSpace = Planet.transform.InverseTransformPoint(worldPos);
            var prevLocalPos = CurrentPos;
            CurrentPos = localSpace;


            // offset cursor by a bit
            var prevPos = Cursor.transform.position;
            var offset = localSpace + localSpace.normalized * CameraDistance;
            Cursor.transform.position = Planet.transform.TransformPoint(offset);

            // ED ROTATION ATTEMPT
            /*
            var xCross = Vector3.Cross(localSpace.normalized, Vector3.left).normalized;
            Cursor.transform.rotation = Quaternion.LookRotation(Vector3.forward, localSpace);
            //Cursor.transform.forward = localSpace.normalized * -1;
            //Cursor.transform.up = newRot * Cursor.transform.up;
            var yawRot = Quaternion.Euler(0f, 0f, _lookRotation);

            var diff = worldPos - Planet.transform.TransformPoint(prevLocalPos);
            var right = Vector3.Cross(worldPos, diff);
            var forward = Vector3.Cross(right, worldPos);
            var newRot = Quaternion.FromToRotation(Vector3.forward, forward);
            newRot *= yawRot;
            Cursor.transform.rotation = newRot;
               */


            // LISA ROTATION ATTEMPT
            //Cursor.transform.up = Quaternion.AngleAxis(_lookRotation, Vector3.forward) * Cursor.transform.up;
            Cursor.transform.rotation =
                Quaternion.AngleAxis(_lookRotation, Vector3.forward) *              // but control the x-Axis
                Quaternion.FromToRotation(new Vector3(0f, 0f, -1f), worldPos); // stay parallel with the planet

            //Cursor.transform.rotation = Quaternion.LookRotation(-1 * worldPos.normalized, Vector3.up);

        }

        private void UpdatePositionCoords(Vector3 worldPos)
        {
            var localSpace = Planet.transform.InverseTransformPoint(worldPos);
            var prevLocalPos = CurrentPos;
            CurrentPos = localSpace;

            // this gets called before GetXAxis / GetYAxis but after cursors gets set
            //GetViewableXYWorldCoords(localSpace, worldPos);

            var height = Planet.GetHeight(localSpace);
            var r = Planet.ShapeSettings.Radius;
            var latLong = LocalSpaceToLatLon(localSpace);
            var lattitude = latLong.x; // - 90 to + 90 up and down
            var longitude = latLong.y; // -180 to + 180 left and right

            var xAxis = GetXAxis(localSpace);
            var yAxis = GetYAxis(localSpace);
            if (LocalNormalDebug)
            {
                LocalNormalDebug.transform.localEulerAngles = yAxis;
            }

            if (DebugText)
            {
                DebugText.text = $"World: {worldPos} Local: {localSpace.normalized}, height: {height} // \n LAT: {lattitude}, LON: {longitude} //\n XAxis: {xAxis} YAxis: {yAxis}";
            }

            var cornerDirections = new Vector2[]
            {
                new Vector2(-0.5f, 0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, -0.5f)
            };

            for (int i = 0; i < Corners.Length; i++)
            {
                var corner = Corners[i];
                var x3D = cornerDirections[i].x * xAxis * ProjectionWidth;
                var y3D = cornerDirections[i].y * yAxis * ProjectionHeight;

                corner.transform.rotation = Quaternion.LookRotation(worldPos.normalized);
                corner.transform.position = x3D + y3D; 
            }


            NoisePreview.UpdatePos(this);
        }

        private Vector3 IncreaseLocalPosBy2DOffset(Vector3 currentLocalPos, Vector2 inputVector)
        {
            var xAngle = Quaternion.AngleAxis(inputVector.x * WASD_Speed, GetXAxis(currentLocalPos));
            var yAngle = Quaternion.AngleAxis(inputVector.y * WASD_Speed, GetYAxis(currentLocalPos));
            return xAngle * yAngle * currentLocalPos;
        }

        public Vector3[] GetViewableXYWorldCoords(Vector3 localPos, Vector3 worldPos)
        {
            _groundPlane = new Plane(worldPos.normalized, worldPos);
            float distance;

            Ray[] rays = new Ray[_viewPort.Length];
            for (int i = 0; i < _viewPort.Length; i++)
            {
                _viewPort[i].z = Camera.farClipPlane;
                var ray = Camera.ViewportPointToRay(_viewPort[i]);

                if (_groundPlane.Raycast(ray, out distance))
                {
                    _viewableXY[i] = ray.GetPoint(distance);
                }
            }

            return _viewableXY;
        }

        public Vector3 GetXAxis(Vector3 localSpace)
        {
            return Cursor.transform.right.normalized;


            var localSpaceP1 = Planet.transform.InverseTransformPoint(_viewableXY[1]);
            var localSpaceP0 = Planet.transform.InverseTransformPoint(_viewableXY[0]);

            return (localSpaceP1 - localSpaceP0).normalized;

            //return Vector3.Cross(localSpace.normalized, Vector3.up).normalized;
        }

        public Vector3 GetYAxis(Vector3 localSpace)
        {
            return Cursor.transform.forward.normalized;

            var localSpaceP3 = Planet.transform.InverseTransformPoint(_viewableXY[3]);
            var localSpaceP0 = Planet.transform.InverseTransformPoint(_viewableXY[0]);

            return -1 * (localSpaceP3 - localSpaceP0).normalized;

            //return Vector3.Cross(xAxis, localSpace.normalized).normalized;
        }

        private Vector2 LocalSpaceToLatLon(Vector3 localSpace)
        {
            var x = localSpace.x;
            var y = localSpace.z;
            var z = localSpace.y;
            var lattitude = Mathf.Atan2(z, Mathf.Sqrt(x * x + y * y)) * Mathf.Rad2Deg;
            var longitute = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            return new Vector2(lattitude, longitute);
        }

        private Vector3 LatLonTo3dSpace(Vector2 latlon, float radius)
        {
            var lattitude = latlon.x * Mathf.Deg2Rad;
            var longitude = latlon.y * Mathf.Deg2Rad;
            return new Vector3
            (
                Mathf.Cos(lattitude) * Mathf.Cos(longitude),
                Mathf.Sin(lattitude),
                Mathf.Cos(lattitude) * Mathf.Sin(longitude)

            ) * radius;
        }
    }
}
