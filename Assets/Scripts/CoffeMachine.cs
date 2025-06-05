using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeMachine : MonoBehaviour
{
		public bool open;
		public Transform Player;
		public ParticleSystem Psys1;
		public ParticleSystem Psys2;
		
		void Start()
		{
			open = false;
		}

		void OnMouseOver()
		{
			{
				if (Player)
				{
					float dist = Vector3.Distance(Player.position, transform.position);
					if (dist < 2)
					{
						if (open == false)
						{
							if (Input.GetMouseButtonDown(0))
							{
								open = true;
								Psys1.Play();
								Psys2.Play();
							}
						}
						else
						{
							if (Input.GetMouseButtonDown(0))
							{
								open = false;
								Psys1.Stop();
								Psys2.Stop();
							}

						}

					}
				}

			}

		}

}
