using UnityEngine;
using UnityEngine.UIElements;

namespace cky.GizmoHelper
{
    public static class GizmoHelper_CKY
    {

        public static void DrawCircle(Transform transform, Vector3 positionOffset, float radius, int segments, Color color)
        {
            Gizmos.color = color;

            var center = transform.position + transform.right * positionOffset.x + transform.up * positionOffset.y + transform.forward * positionOffset.z;

            float angleStep = 360f / segments;

            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad; // A�� radyan cinsine �evrildi
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                Vector3 newPoint = new Vector3(x, 0, z) + center; // Pozisyonu merkez ile hesapla

                if (i == 0)
                {
                    firstPoint = newPoint; // �lk noktay� kaydet
                }
                else
                {
                    Gizmos.DrawLine(prevPoint, newPoint); // Bir �nceki noktadan yeni noktaya �izgi �iz
                }

                prevPoint = newPoint; // Yeni noktay� bir �nceki nokta olarak g�ncelle
            }

            // Son noktadan ilk noktaya �izgi �ekerek daireyi kapat
            Gizmos.DrawLine(prevPoint, firstPoint);
        }



        public static void DrawCirclePie(Transform transform, Vector3 direction, float radius, float angle, int segments, Color color)
        {
            // Kullan�c�n�n belirledi�i do�rultuyu nesnenin rotasyonuna g�re ayarla
            Vector3 forward = transform.rotation * direction.normalized;

            // A��n�n iki kenar�n� hesapla
            Quaternion leftRotation = Quaternion.Euler(0, -angle / 2, 0);
            Quaternion rightRotation = Quaternion.Euler(0, angle / 2, 0);

            // Sol ve sa� y�n vekt�rlerini olu�tur
            Vector3 leftDirection = leftRotation * forward;
            Vector3 rightDirection = rightRotation * forward;

            // Y�n oklar�n� �iz
            //Gizmos.DrawLine(transform.position, transform.position + forward * radius);
            Gizmos.DrawLine(transform.position, transform.position + leftDirection * radius);
            Gizmos.DrawLine(transform.position, transform.position + rightDirection * radius);

            // A�� alan�n� �iz
            Gizmos.color = new Color(1, 0, 0, 0.5f); // Yar�m saydam k�rm�z�
            Vector3[] corners = new Vector3[3];

            corners[0] = transform.position;
            corners[1] = transform.position + leftDirection * radius;
            corners[2] = transform.position + rightDirection * radius;

            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[0], corners[2]);

            // �ember �izimi (sadece a��n�n g�sterdi�i k�s�mda)

            // �emberin ba�lang�� ve biti� a��s�n� hesapla
            float startAngle = -angle / 2;
            float endAngle = angle / 2;

            Vector3 previousPoint = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float angleStep = (endAngle - startAngle) / segments; // A�� ad�m�n� hesapla
                float angleRad = (startAngle + angleStep * i) * Mathf.Deg2Rad; // Radyan cinsine �evir
                Vector3 point = transform.position + new Vector3(Mathf.Cos(angleRad) * radius, 0, Mathf.Sin(angleRad) * radius);
                if (i > 0)
                {
                    Gizmos.DrawLine(previousPoint, point);
                }
                previousPoint = point;
            }
        }

    }
}
