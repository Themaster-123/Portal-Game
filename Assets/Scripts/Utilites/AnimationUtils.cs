using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Utilites
{
	public class AnimationUtils : MonoBehaviour
	{
		public delegate void AnimationFunction(float time, float endTime, ref Vector3 position, ref Quaternion rotation);

		protected static AnimationUtils instance;
		protected static Dictionary<int, UtilAnimation> animations = new Dictionary<int, UtilAnimation>();

		public static void Rotate360(Transform transform, Vector3 axis, float speed)
		{
			Animate(transform, speed, (float time, float endTime, ref Vector3 position, ref Quaternion rotation) =>
			{
				if (time + Time.deltaTime >= endTime)
				{
					rotation = Quaternion.identity;
				}
				else
				{
					rotation = Quaternion.AngleAxis(360 * time * speed, axis);
				}
			}, () => { });
		}

		public static void PingPongRotate(Transform transform, Vector3 axis, float maxAngle, float speed)
		{
			Animate(transform, speed, (float time, float endTime, ref Vector3 position, ref Quaternion rotation) =>
			{
				if (time + Time.deltaTime >= endTime)
				{
					rotation = Quaternion.identity;
				}
				else
				{
					float normalizedSpeed = time * speed;
					float direction;
					float pingPongTime;

					if (time > endTime / 2)
					{
						direction = -1;
						pingPongTime = normalizedSpeed - 1;
					}
					else
					{
						direction = 1;
						pingPongTime = normalizedSpeed;
					}

					rotation = Quaternion.AngleAxis(maxAngle * 2 * pingPongTime * direction, axis);
				}
			}, () => { });
		}

		public static void PingPongMove(Transform transform, Vector3 direction, float distance, float speed)
		{
			Animate(transform, speed, (float time, float endTime, ref Vector3 position, ref Quaternion rotation) =>
			{
				if (time + Time.deltaTime >= endTime)
				{
					rotation = Quaternion.identity;
				}
				else
				{
					float normalizedSpeed = time * speed;
					float timeDirection;
					float pingPongTime;

					if (time > endTime / 2)
					{
						timeDirection = -1;
						pingPongTime = normalizedSpeed - 1;
					}
					else
					{
						timeDirection = 1;
						pingPongTime = normalizedSpeed;
					}

					position = direction * distance * 2 * pingPongTime * timeDirection;
				}
			}, () => { });
		}

		public static void Animate(Transform transform, float speed, AnimationFunction action, Action endAction)
		{
			int id = transform.GetInstanceID();
			int animationCount = 0;

			if (!animations.TryGetValue(id, out UtilAnimation utilAnimation))
			{
				utilAnimation = new UtilAnimation(transform, new List<IEnumerator> { });
				animations.Add(id, utilAnimation);
			}

			animationCount = utilAnimation.animations.Count;

			IEnumerator enumerator = null;

			endAction += () =>
			{
				utilAnimation.animations.Remove(enumerator);
				if (utilAnimation.animations.Count == 0)
				{
					if (transform == null)
					{
						animations.Remove(id);
					}
					else
					{
						StopAnimation(transform);
					}
				}
			};

			enumerator = EAnimate(transform, speed, action, endAction);

			utilAnimation.animations.Add(enumerator);

			Coroutine coroutine = instance.StartCoroutine(enumerator);
		}

		public static void StopAnimation(Transform transform)
		{
			if (transform == null) return;

			if (animations.TryGetValue(transform.GetInstanceID(), out UtilAnimation utilAnimation))
			{
				foreach (IEnumerator coroutine in utilAnimation.animations)
				{
					instance.StopCoroutine(coroutine);
				}
				transform.localPosition = utilAnimation.orginalPosition;
				transform.localRotation = utilAnimation.orginalRotation;
				animations.Remove(transform.GetInstanceID());
			}
		}

		protected void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
		}

		protected static IEnumerator EAnimate(Transform transform, float speed, AnimationFunction action, Action endAction)
		{
			float endTime = 1 / speed;
			Vector3 animatePosition = Vector3.zero;
			Quaternion animateRotation = Quaternion.identity;

			for (float time = 0; time <= endTime; time += Time.deltaTime)
			{
				Vector3 oldAnimatePosition = animatePosition;
				Quaternion oldAnimateRotation = animateRotation;
				action(time, endTime, ref animatePosition, ref animateRotation);
				if (transform == null) goto AnimateEnd;
				transform.localPosition = transform.localPosition - oldAnimatePosition + animatePosition;
				transform.localRotation = transform.localRotation * Quaternion.Inverse(oldAnimateRotation) * animateRotation;
				yield return null;
			}

			if (transform == null) goto AnimateEnd;
			transform.localPosition = transform.localPosition - animatePosition;
			transform.localRotation = transform.localRotation * Quaternion.Inverse(animateRotation);

			AnimateEnd:
			endAction();


		}

		protected class UtilAnimation
		{
			public Vector3 orginalPosition;
			public Quaternion orginalRotation;
			public List<IEnumerator> animations;
			public UtilAnimation(Vector3 orginalPosition, Quaternion orginalRotation, List<IEnumerator> coroutines)
			{
				this.orginalPosition = orginalPosition;
				this.orginalRotation = orginalRotation;
				this.animations = coroutines;
			}

			public UtilAnimation(Transform transform, List<IEnumerator> coroutines) : this(transform.localPosition, transform.localRotation, coroutines)
			{
			}
		}
	}

}