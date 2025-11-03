using System;
using UnityEngine;

public class Card : MonoBehaviour
{
    private Vector3 offset;
    private Camera camera;
    private Vector3 homePos = Vector3.zero;
    private CardSequence parent;
    public float speedScale = 0.1f;
    public Sprite cardSprite;

    private bool isDragin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardSprite;
        camera = Camera.main;
        // homePos = new Vector3(transform.position.x,  transform.position.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragin) return;
        moveTo(homePos, Time.deltaTime);
        // transform.position = homePos;
    }

    private void OnMouseDown()
    {
        isDragin = true;
        offset = transform.position - camera.ScreenToWorldPoint(Input.mousePosition);
        offset.z = 0;
        parent.removeCard(this);
    }
    private void OnMouseDrag()
    {
        Vector3 targetPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = -0.1f;
        
        // transform.position = targetPosition + offset;
        moveTo(targetPosition + offset, Time.deltaTime);
    }

    private void OnMouseUp()
    {
        if (transform.position.y < parent.terminator)
        {
            GameObject buf = GameObject.Find("Downer Sequence");
            parent = buf.GetComponent<CardSequence>();
            parent.addCard(this);
        }
        else
        {
            GameObject buf = GameObject.Find("Upper Sequence");
            parent = buf.GetComponent<CardSequence>();
            parent.addCard(this);
        }
        isDragin = false;
    }

    public void setParent(CardSequence parent) => this.parent = parent;
    public void setHomePos(Vector3 pos) => homePos = pos;

    private void moveTo(Vector3 pos, float deltaTime, float z = 0)
    {
        if (pos.x == transform.position.x && pos.y == transform.position.y) return;
        Vector3 target = pos - transform.position;
        target = target.normalized * (target.magnitude * deltaTime / speedScale);
        transform.position += target;
        transform.position.Set(transform.position.x, transform.position.y, z);
    }
}
