using TMPro;
using UnityEngine;

namespace Game
{
    public class PointsPopup : MonoBehaviour
    {
        public static PointsPopup Create(Vector3 position, int amount, bool coins = false)
        {
            //static class to spawn a points pop-up
            if (amount != 0)
            {
                GameObject pointsPopupObject =
                    Instantiate(GameMode.instance.pointsPopup, position, Quaternion.identity);

                PointsPopup pointsPopup = pointsPopupObject.GetComponent<PointsPopup>();
                pointsPopup.Setup(amount, coins);

                return pointsPopup;
            }

            return null;
        }

        public static PointsPopup CreateTextPopup(Vector3 position, string message, Color color)
        {
            //static class to display text pop-up
            if (message != "")
            {
                GameObject pointsPopupObject =
                    Instantiate(GameMode.instance.pointsPopup, position, Quaternion.identity);

                PointsPopup pointsPopup = pointsPopupObject.GetComponent<PointsPopup>();
                pointsPopup.MessageSetup(message, color);

                return pointsPopup;
            }

            return null;
        }

        private static int sortingOrder;

        private const float DISAPPEAR_TIMER_MAX = 1f;

        private TextMeshPro textMesh;
        private float dissapearTimer;
        private Color textColor;
        private Vector3 moveVector;

        private void Awake()
        {
            textMesh = transform.GetComponent<TextMeshPro>();
        }

        public void Setup(int amount, bool coins)
        {
            //sets the color + text of the pop-up + starts the upwards motion
            if (coins)
            {
                textMesh.color = Color.yellow;
                textMesh.SetText("+" + amount + "$");
            }
            else
            {
                textMesh.color = Color.white;
                textMesh.SetText(amount.ToString());
            }

            textColor = textMesh.color;
            dissapearTimer = DISAPPEAR_TIMER_MAX;
            moveVector = new Vector3(0.7f, 1) * 60f;

            textMesh.sortingOrder = ++sortingOrder;
        }

        public void MessageSetup(string message, Color color)
        {
            //sets the color, message and lifetime of a text pop-up
            textMesh.color = color;
            textMesh.SetText(message);
            textColor = textMesh.color;
            dissapearTimer = DISAPPEAR_TIMER_MAX;
            moveVector = new Vector3(0.7f, 1) * 60f;

            textMesh.sortingOrder = ++sortingOrder;
        }

        private void Update()
        {
            //slowly moves the pop-up upwards + scales down over time and dissapears
            transform.position += moveVector * Time.deltaTime;
            moveVector -= Time.deltaTime * 8f * moveVector;

            if (dissapearTimer > DISAPPEAR_TIMER_MAX / 2)
            {
                //First half of lifetime
                float increaseScaleAmount = 1f;
                transform.localScale += Time.deltaTime * increaseScaleAmount * Vector3.one;
            }
            else
            {
                //Second half of lifetime
                float decreaseScaleAmount = 1f;
                transform.localScale -= Time.deltaTime * decreaseScaleAmount * Vector3.one;
            }

            dissapearTimer -= Time.deltaTime;

            if (dissapearTimer < 0)
            {
                //Start disappearing
                float disappearSpeed = 3f;
                textColor.a -= disappearSpeed * Time.deltaTime;
                textMesh.color = textColor;

                if (textColor.a < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}