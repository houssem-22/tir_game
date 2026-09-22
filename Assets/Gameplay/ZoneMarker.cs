using UnityEngine;

namespace Cipher.Gameplay
{
    public sealed class ZoneMarker : MonoBehaviour
    {
        [SerializeField] string zoneName = "ZONE";
        TextMesh _mesh;

        public void Setup(string name, Color color)
        {
            zoneName = name;

            var labelGo = new GameObject("ZoneLabel");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 3.4f / Mathf.Max(0.01f, transform.localScale.y), 0f);
            labelGo.transform.localScale = new Vector3(
                1f / Mathf.Max(0.01f, transform.localScale.x),
                1f / Mathf.Max(0.01f, transform.localScale.y),
                1f / Mathf.Max(0.01f, transform.localScale.z));
            _mesh = labelGo.AddComponent<TextMesh>();
            _mesh.text = zoneName;
            _mesh.characterSize = 0.18f;
            _mesh.fontSize = 48;
            _mesh.anchor = TextAnchor.MiddleCenter;
            _mesh.alignment = TextAlignment.Center;
            _mesh.color = color;
        }

        void LateUpdate()
        {
            if (_mesh == null || Camera.main == null) return;
            _mesh.transform.rotation = Quaternion.LookRotation(
                _mesh.transform.position - Camera.main.transform.position);
        }
    }
}
