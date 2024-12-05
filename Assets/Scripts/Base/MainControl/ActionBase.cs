using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IAction
{
    void MoveComponent();
    void Attack();
    void Die();
}
