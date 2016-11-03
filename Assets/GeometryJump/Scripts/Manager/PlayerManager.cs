/***********************************************************************************************************
 * Produced by App Advisory - http://app-advisory.com													   *
 * Facebook: https://facebook.com/appadvisory															   *
 * Contact us: https://appadvisory.zendesk.com/hc/en-us/requests/new									   *
 * App Advisory Unity Asset Store catalog: http://u3d.as/9cs											   *
 * Developed by Gilbert Anthony Barouch - https://www.linkedin.com/in/ganbarouch                           *
 ***********************************************************************************************************/




using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

/// <summary>
/// Class in charge to manage the player
/// 负责管理类的成员
/// </summary>
namespace AppAdvisory.GeometryJump
{
	public class PlayerManager : MonoBehaviorHelper
	{
        //跳跃次数
		public int jumpCount = 0;
        //是否可以跳跃
		private bool canJump;
        //游戏是否结束
		private bool isGameOver;

        //呈现一个精灵体2d图形。
        public SpriteRenderer mask;
		public List<Sprite> listMask;


        /// <summary>
        /// PlayerPrefs 存储和访问玩家游戏会话之间的偏好。
        /// </summary>
        /// <param name="s"></param>
		public void SetMask(Sprite s)
		{
			if(s == null)
			{
				print("mask 0");
				PlayerPrefs.SetInt("MASK",0);
				PlayerPrefs.Save();
				return;
			}

			for(int i = 1; i < listMask.Count; i++)
			{
				if(string.Equals(s.name, listMask[i].name))
				{
					print("mask " + i);
					PlayerPrefs.SetInt("MASK",i);
					PlayerPrefs.Save();
					return;
				}
			}
		}
		void ChangePlayerMask()
		{
			Sprite s = null;
			int num = PlayerPrefs.GetInt("MASK",0);
			s = listMask[num];
			mask.sprite = s;
			mask.gameObject.SetActive(s != null);
		}
		void OnAnimationTransitionOutStart()
		{
			if(mask.gameObject.activeInHierarchy)
			{
				mask.transform.localScale = Vector2.one * 30;
				mask.transform.DOScale(1.2f * Vector2.one,1);
				mask.transform.DOLocalRotate(-Vector3.forward * 360,1,RotateMode.FastBeyond360);
			}

			CanvasManager.OnAnimationTransitionOutStart -= OnAnimationTransitionOutStart;
		}
        //开始跳跃的代理
		public delegate void PlayerJumpStart();
        //开始跳跃的事件
		public static event PlayerJumpStart OnPlayerJumpStarted;

        //跳跃结束的代理
		public delegate void PlayerJumpEnd();
        //跳跃结束的事件
		public static event PlayerJumpEnd OnPlayerJumpEnded;

        //
		public delegate void PlayerJump(float animTime, float distX, float distY, float jumpHeight);
		public static event PlayerJump OnPlayerJump;

		void OnEnable()
		{
            // += 增加事件的处理方法
            InputTouch.OnTouchLeft += OnTouchLeft;
			InputTouch.OnTouchRight += OnTouchRight;
			CanvasManager.OnAnimationTransitionOutStart += OnAnimationTransitionOutStart;
		}

		void OnDisable()
		{
			InputTouch.OnTouchLeft -= OnTouchLeft;
			InputTouch.OnTouchRight -= OnTouchRight;
			CanvasManager.OnAnimationTransitionOutStart -= OnAnimationTransitionOutStart;
		}

		void OnTouchLeft()
		{
			DoMove(true);
		}

		void OnTouchRight()
		{
			DoMove(false);
		}

		void Awake()
		{
			listMask = new List<Sprite>();

			var maskIcons = canvasManager.gameObject.GetComponentsInChildren<MaskIcon>(true);

			foreach(var m in maskIcons)
			{
				Sprite s = null;
				var i = m.spriteMask;
				if(i != null)
				{
					s = i.sprite;
				}
				listMask.Add(s);
			}

			ChangePlayerMask();
		}

		void Start()
		{
			isGameOver = false;
			canJump = true;
		}

		public void LaunchGameOver()
		{
			if (isGameOver)
				return;

			isGameOver = true;

			StopAllCoroutines();

            //启动一个协同进程
			StartCoroutine (CoroutLaunchGameOver ());
		}

        /// <summary>
        /// IEnumerator 支持对泛型集合的简单迭代
        /// </summary>
        /// <returns></returns>
		IEnumerator CoroutLaunchGameOver()
		{
			gameManager.GameOverStart ();

			soundManager.PlayMusicGameOver ();

			yield return 0;

			StartCoroutine(CameraShake.Shake(Camera.main.transform,0.1f));

			yield return new WaitForSeconds(1f);

			gameManager.GameOverEnd ();

			float height = 2f * cam.orthographicSize;

			float finalPosY = transform.position.y - height;

            //类变换的Y位置给定的值。还存储转换为渐变的目标,因此它可以用于过滤操作
            transform.DOMoveY(finalPosY, 1f).SetEase(Ease.Linear);
		}


		public void Continue()
		{
			isGameOver = false;

			canJump = true;

			continuousMove.Reposition();


			var pl = gameManager.gameObject.GetComponentsInChildren<PlatformLogic>(false);

			var oldPos = transform.position;

			var p = pl[0];

			foreach(var pp in pl)
			{
				if(pp.transform.position.x < p.transform.position.x && pp.transform.position.x > transform.position.x)
					p = pp;
			}

			transform.position = p.transform.position + Vector3.up * 0.5f;



			mainCameraManager.UpdatePos (oldPos, transform.position);

		}

        // NonSerialized 初始化 System.NonSerializedAttribute 类的新实例。
        [NonSerialized] public float animTime = 0.12f;

		bool isDoingMoveX = false;

		bool isDoingMoveY = false;

		bool isDoingRotate = false;

		bool isMoving
		{
			get
			{
				return isDoingMoveX || isDoingMoveY || isDoingRotate;
			}
		}

		public Transform spriteTransform;
		public Transform shadowTransform;



		float GetFinalPosX(bool isSmall)
		{
			float startPosX = transform.position.x;

			float finalPosX = startPosX + (isSmall ?  gameManager.smallSpace : 2*gameManager.smallSpace);

			return finalPosX;
		}


		float GetFinalPosY(bool isSmall)
		{
			float startPosY = transform.position.y;

			float finalPosY = startPosY + (isSmall ? 2 : 4);

			return finalPosY;
		}

		Vector3 GetFinalVector(bool isSmall)
		{
			return new Vector3(GetFinalPosX(isSmall),GetFinalPosY(isSmall),0);
		}

		void DoMove(bool isSmall)
		{
			if (!canJump || isGameOver)
				return;

			if(isMoving)
				return;

			soundManager.PlayJumpFX();
            // Physics2D 全局设置和2d物理助手。
            var hit = Physics2D.Raycast(transform.position,-Vector2.up,1);
			if(hit)
			{
				var platformLogic = hit.collider.GetComponent<PlatformLogic>();
				if(platformLogic != null)
					platformLogic.DoMove();
				else
					Debug.LogWarning("error!!");
			}

			mainCameraManager.UpdatePos (transform.position, GetFinalVector(isSmall));

			if(OnPlayerJumpStarted != null)
				OnPlayerJumpStarted();

			if(OnPlayerJump != null)
				OnPlayerJump(animTime, GetFinalPosX(isSmall) - transform.position.x, GetFinalPosY(isSmall) - transform.position.y, jumpHeight);

			DoMoveY(isSmall, () => {
				if(!isMoving)
					CheckIfOnPlatform(isSmall);
			});
			DoMoveX(isSmall, () => {
				if(!isMoving)
					CheckIfOnPlatform(isSmall);
			});
			DoRotate(isSmall, () => {
				if(!isMoving)
					CheckIfOnPlatform(isSmall);
			});
		}


        /// <summary>
        /// 检查是否落在平台上
        /// </summary>
        /// <param name="isSmall"></param>
		void CheckIfOnPlatform(bool isSmall)
		{
			if(OnPlayerJumpEnded != null)
				OnPlayerJumpEnded();

			var hit = Physics2D.Raycast(transform.position,-Vector2.up,1);

			if(hit)
			{
				gameManager.AddPoint(isSmall ? 1 : 2);

			}
			else
			{

				LaunchGameOver();
			}
		}

		float jumpHeight = 0.2f;

		void DoMoveY(bool isSmall, Action callback)
		{
			float startPosY = transform.position.y;

			float finalPosY = GetFinalPosY(isSmall);

			float timeJump = animTime / 2f;

			var pos = transform.position;
			pos.y = startPosY;
			transform.position = pos;

			isDoingMoveY = true;

			transform.DOMoveY(finalPosY + jumpHeight, timeJump)
				.OnUpdate(() => {
					isDoingMoveY = true;
				})
				.OnComplete(() =>{
					transform.DOMoveY(finalPosY, timeJump)
						.SetEase(Ease.Linear)
						.OnUpdate(() => {
							isDoingMoveY = true;
						})
						.OnComplete(() =>{
							pos = transform.position;
							pos.y = finalPosY;
							transform.position = pos;

							isDoingMoveY = false;

							if(callback != null)
								callback();
						});
				});
		}

		void DoMoveX(bool isSmall, Action callback)
		{
			float startPosX = transform.position.x;

			float finalPosX = GetFinalPosX(isSmall);

			float timeJump = animTime;

			var pos = transform.position;
			pos.x = startPosX;
			transform.position = pos;

			isDoingMoveX = true;
            //类变换的X位置给定的值。还存储转换为渐变的目标,因此它可以用于过滤操作
            transform.DOMoveX(finalPosX, timeJump)
				.OnUpdate(() => {
					isDoingMoveX = true;
				})
				.OnComplete(() => {
					pos = transform.position;
					pos.x = finalPosX;
					transform.position = pos;

					isDoingMoveX = false;

					if(callback != null)
						callback();
				});
		}


		void DoRotate(bool isSmall, Action callback)
		{

			float startRot = spriteTransform.eulerAngles.z;

			float finalRot = startRot - 90;

			float timeJump = animTime;

			isDoingRotate = true;

			var rot = spriteTransform.eulerAngles;
			rot.z = startRot;
			spriteTransform.eulerAngles = rot;
			shadowTransform.eulerAngles = spriteTransform.eulerAngles;

			spriteTransform.DORotate(Vector3.forward * finalRot, timeJump, RotateMode.FastBeyond360)
			//			.SetEase(Ease.Linear)
				.OnUpdate(() => {
					isDoingRotate = true;
					shadowTransform.eulerAngles = transform.eulerAngles;
				})
				.OnComplete(()=>{
					rot = transform.eulerAngles;
					rot.z = finalRot;
                    /**
                    unity中的欧拉角有两种方式可以解释:
                        1,当认为顺序是yxz时（其实就是heading - pitch - bank），是传统的欧拉角变换，也就是以物体自己的坐标系为轴的。
                        2，当认为顺序是zxy时（roll - pitch - yaw），也是官方文档的顺序时，是以惯性坐标系为轴的。后者比较直观一些，但其实两者的实际效果是一样的，只是理解不一样。

                    */
                    // eulerAngles 欧拉角的旋转度。
                    spriteTransform.eulerAngles = rot;
					shadowTransform.eulerAngles = transform.eulerAngles;
					isDoingRotate = false;

					if(callback != null)
						callback();
				});
		}
	}
}