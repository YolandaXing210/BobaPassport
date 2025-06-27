using UnityEngine;

namespace BobaShooter
{
    /// <summary>
    /// Visualizes the touch detection area in the editor and optionally in play mode
    /// </summary>
    [RequireComponent(typeof(BobaShootingController))]
    public class TouchAreaVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showInPlayMode = true;
        [SerializeField] private Color areaColor = new Color(0, 1, 0, 0.2f);
        [SerializeField] private Color borderColor = new Color(0, 1, 0, 0.8f);
        
        private BobaShootingController shootingController;
        private Rect touchArea;
        
        private void Start()
        {
            shootingController = GetComponent<BobaShootingController>();
            UpdateTouchArea();
        }
        
        private void UpdateTouchArea()
        {
            // Get the touch area bounds from the shooting controller
            shootingController.GetTouchAreaBounds(out float xMin, out float xMax, out float yMin, out float yMax);
            
            float screenXMin = xMin * Screen.width;
            float screenXMax = xMax * Screen.width;
            float screenYMin = yMin * Screen.height;
            float screenYMax = yMax * Screen.height;
            
            // Note: Unity GUI coordinates have Y=0 at top, but Input has Y=0 at bottom
            // So we need to flip Y for display
            touchArea = new Rect(screenXMin, Screen.height - screenYMax, 
                                screenXMax - screenXMin, screenYMax - screenYMin);
        }
        
        private void OnGUI()
        {
            if (!showInPlayMode && Application.isPlaying) return;
            
            UpdateTouchArea();
            
            // Draw filled area
            GUI.color = areaColor;
            GUI.DrawTexture(touchArea, Texture2D.whiteTexture);
            
            // Draw border
            GUI.color = borderColor;
            DrawRectBorder(touchArea, 2);
            
            // Draw label
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            GUI.Label(touchArea, "SWIPE DOWN HERE", style);
        }
        
        private void DrawRectBorder(Rect rect, float thickness)
        {
            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), Texture2D.whiteTexture);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        }
    }
}