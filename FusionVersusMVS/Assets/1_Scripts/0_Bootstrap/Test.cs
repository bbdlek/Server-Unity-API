using System;
using Fusion;

namespace _1_Scripts._0_Bootstrap
{
    public class Test : NetworkBehaviour
    {
        [Networked] public float Health { get; set; } = 100;
        [Networked] public float Health2 { get; set; } = 100;

        private void Start()
        {
            Health = Health2;
        }
    }
}