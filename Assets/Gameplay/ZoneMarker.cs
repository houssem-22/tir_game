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
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = color;
            }

            var labelGo = new GameObject("ZoneLabel");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _mesh = labelGo.AddComponent<TextMesh>();
            _mesh.text = zoneName;
            _mesh.characterSize = 0.25f;
            _mesh.fontSize = 64;
            _mesh.anchor = TextAnchor.MiddleCenter;
            _mesh.color = Color.white;
        }
    }
}
