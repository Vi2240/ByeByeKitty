using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    private Material material;

    private void Awake()
    {
        material = new Material(playerSpriteRenderer.material);
    }

    public void SetPlayerColor(Color color) // or SetPlayerSprite: the options from CharecterOptionSelectUI
    {
        material.color = color;
    }

}