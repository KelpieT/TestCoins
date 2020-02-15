using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChipsController
{

    public class ChipsController : MonoBehaviour
    {
        private Mesh chip;
        private void Start()
        {
            LoadAllResources();
        }
        private void LoadAllResources()
        {
            chip = Resources.Load<Mesh>("Models/Chip");
            //Debug.Log(chip.ToString()); 
        }
        private void GenerateStack(int CountChip) { }
        private void SplitStack(int CountChip) { }
    }
}