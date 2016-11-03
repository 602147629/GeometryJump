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
using DG.Tweening;

/// <summary>
/// Class in charge to move the big hazard on the left continuously during the game
/// </summary>
namespace AppAdvisory.GeometryJump
{
	public class ContinuousMove : MonoBehaviorHelper 
	{

		public bool BLOCK_ME = false;

		public Transform spikes;
		public Transform spikeToClone;


		void OnEnable()
		{
            //加入委托
			GameManager.OnGameStart += DoMove;
			GameManager.OnGameOverStarted += StopMove;
			GameManager.OnSetPoint += OnSetPoint;
			PlayerManager.OnPlayerJumpStarted += DoMove;
		}

		void OnDisable()
		{
            //取消委托
			GameManager.OnGameStart -= DoMove;
			GameManager.OnGameOverStarted -= StopMove;
			GameManager.OnSetPoint -= OnSetPoint;
			PlayerManager.OnPlayerJumpStarted -= DoMove;
		}

		bool isMoving = false;

        /// <summary>
        /// 停止移动
        /// </summary>
		public void StopMove()
		{
			isMoving = false;
		}

		public void DoMove()
		{
			isMoving = true;
		}


		float speed = 1;

		void OnSetPoint(int p)
		{
			speed++;
		}

		public 	List<Transform> l;

		void Start()
		{
            // 复位
			Reposition();

			l = new List<Transform>();

			int i = 1;

			l.Add(spikeToClone.transform);

			while(spikes.childCount < 200)
			{
                //as 和 is 都是强制类型转换， 如果obj不兼容某一类型，as操作符会返回null。
                var t1 = Instantiate(spikeToClone.gameObject) as GameObject;
				var t2 = Instantiate(spikeToClone.gameObject) as GameObject;

				t1.transform.parent = spikes;
				t2.transform.parent = spikes;

				l.Add(t1.transform);
				l.Add(t2.transform);

				t1.transform.localScale = spikeToClone.localScale;
				t2.transform.localScale = spikeToClone.localScale;

                // Vector3 创建一个新的向量和给定的x,y,z的值。
                t1.transform.localPosition = new Vector3(i * -0.0163f, 0, 0);
				t2.transform.localPosition = new Vector3(i * +0.0163f, 0, 0);

                //旋转
				t1.transform.localRotation = spikeToClone.localRotation;
				t2.transform.localRotation = spikeToClone.localRotation;

				i++;
			}

			l.Sort(delegate(Transform c1, Transform c2){
				//			return Vector3.Distance(this.transform.position, c1.transform.position).CompareTo
				//				((Vector3.Distance(this.transform.position, c2.transform.position)));   
				return c1.position.y.CompareTo(c2.position.y);
			});


			for(int j = 0; j < spikes.childCount; j++)
			{
				Transform t = l[j];
                //设置对应成员的索引。
				t.SetSiblingIndex(j);
				t.GetComponent<SpriteRenderer>().sortingOrder = 5 + t.GetSiblingIndex();
			}

			DoMoveSpikes();

		}



		float GetDistance()
		{
			float d = playerManager.transform.position.x - transform.position.x;
			return d;
		}

		void Update() 
		{

			if(BLOCK_ME)
			{
				return;
			}

			if(!isMoving)
				return;

            // 沿着x轴移动变换x,y沿着y轴,z沿着z轴。
            transform.Translate((GetDistance() + 5 + speed / 100f) * Time.deltaTime, 0, 0, Camera.main.transform);


			Vector2 pos = transform.position;

            // Mathf 常见的数学函数的集合。
            //public static float Lerp(float a, float b, float t);
            // 线性插入t 在a和b之间。
            pos.y = Mathf.Lerp(transform.position.y, playerManager.transform.position.y, Time.deltaTime);


			transform.position = pos;
		}

		public void Reposition()
		{
			float height = 2f * cam.orthographicSize;
			float width = height * cam.aspect;

			var p = transform.position;
			p.x = cam.transform.position.x - width/2f;
			transform.position = p;
		}

        //进入2D模式
		void OnTriggerEnter2D(Collider2D other) 
		{
			var platform = other.GetComponent<PlatformLogic>();

			if(platform != null)
			{
				platform.DoMove();
				return;
			}

			var player = other.GetComponent<PlayerManager>();
			if(player != null)
			{
				player.LaunchGameOver();
				return;
			}
		}


        /// <summary>
        /// 重新开始 
        /// </summary>
		public void DoContinueRestart()
		{
			transform.position += 2 * Vector3.down;
		}

		void DoMoveSpikes()
		{
			var startposition = -0.0163f/3f;

			var endPosition = +0.0163f/3f;

			float timeMove = 0.3f;

			var pos = spikes.localPosition;
			pos.x = startposition;
			spikes.localPosition = pos;

            // 杀死所有吐温类这一目标作为参考(意思吞世代从这个目标,或者加入了这一目标作为一个Id)并返回吞世代死亡的总数。
            spikes.DOKill();
            // 吐温类变换的X localPosition给定的值。还存储转换为渐变的目标,因此它可以用于过滤操作
            spikes.DOLocalMoveX(endPosition, timeMove)
			//			.SetEase(Ease.Linear)
				.SetLoops(-1,LoopType.Yoyo);

		}
	}
}