using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Shine : MonoBehaviour
{
    public bool isShining = true;
    public SpriteRenderer shiningPrefab;
    private Animator animator;
    private PolygonCollider2D polygonCollider;

    private bool hasBeenClicked = false;

    private void Start()
    {
        shiningPrefab = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        if (isShining)
        {
            shiningPrefab.color = Color.yellow;
        }
    }

    private void OnMouseDown()
    {
        //if (hasBeenClicked) return;

        hasBeenClicked = true;

        if (animator != null)
        {
            animator.SetTrigger("PlayAnimation");

        }
            shiningPrefab.color = new Color(1f, 1f, 1f);
    }
}
