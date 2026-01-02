using Fungus;
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
    public Flowchart flowchart;

    public bool HasBeenClicked { get; private set; }
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
        bool dialogIsEnd = flowchart.GetBooleanVariable("isEnd");
        if (dialogIsEnd == false) return;

        HasBeenClicked = true;

        if (animator != null)
        {
            animator.SetTrigger("PlayAnimation");

        }
        shiningPrefab.color = new Color(1f, 1f, 1f);
        DialogController.Instance.CheckLevelCompletion();
    }
}
