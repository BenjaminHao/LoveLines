using UnityEngine;

public class BGColorManager : MonoBehaviour
{

    public Color[] Colors;
    public float speed = 5f;
    int _currentIndex = 0;
    int _newIndex = 0;
    Camera _cam;
    bool _shouldChange = false;

	// Use this for initialization
	void Start () 
    {
        _cam = GetComponent<Camera>();
        SetColor(Colors[_currentIndex]);
	}
	
    public void SetColor(Color color)
    {
        _cam.backgroundColor = color;
    }

    public void ChangeColor(int index)
    {
        _shouldChange = true;
        _newIndex = index;
    }

	// Update is called once per frame
	void Update () 
    {
        if (_shouldChange)
        {
            var startColor = _cam.backgroundColor;
            var endColor = Colors[_newIndex];

            var newColor = Color.Lerp(startColor, endColor, Time.deltaTime*speed);
            SetColor(newColor);

            _shouldChange &= newColor != endColor;
        }
	}
}
