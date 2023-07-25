using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Mono.Cecil.Cil;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;
    private bool _follow = false;

    public float cameraSpeed;
    public float cameraZoomRate = 2f;
    public float cameraZoomMinimum = 5f;
    public float cameraZoomMaximum = 100f;

    private Vector3 dragOrigin;
    [SerializeField] private OrganismStatView organismStatView;
    
    // Start is called before the first frame update
    void Start()
    {
        
        _camera = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            _camera.m_Lens.OrthographicSize -= cameraZoomRate * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _camera.m_Lens.OrthographicSize += cameraZoomRate * Time.deltaTime;
        }

        if (_camera.m_Lens.OrthographicSize < cameraZoomMinimum)
        {
            _camera.m_Lens.OrthographicSize = cameraZoomMinimum;
        }
        
        if (_camera.m_Lens.OrthographicSize > cameraZoomMaximum)
        {
            _camera.m_Lens.OrthographicSize = cameraZoomMaximum;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _follow = !_follow;
        }

        

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        transform.position += new Vector3(horizontal, vertical, 0).normalized * (cameraSpeed * (_camera.m_Lens.OrthographicSize / cameraZoomMinimum) * Time.deltaTime);
        

        if (!_follow)
        {
            _camera.Follow = null;
        }
        
        if (_follow)
        {
            if (organismStatView.target != null)
            {
                _camera.Follow = organismStatView.target.transform;
            }
            else
            {
                _follow = false;
            }
        }
    }
}
