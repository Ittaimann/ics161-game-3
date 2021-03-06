﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    // private variables 
        
	private Rigidbody2D rb2d;
    private float health;
    private float speed;
    private bool can_take_damage = true, damage_wait = false;
    private bool shooting = false;
    private ParticleSystem shoot_ps;
    private AudioSource powerupAudio;
    public AudioSource shootAudio;
    private int bulletPickups;


    public int otherPlayerIndex;

    //should get this from the gun
    private float shoot_wait = 1;
    private int bullets_to_shoot = 1;
    private float spread = 2;
    private float damage = 1;
    private float bulletSpeed = 50;
    private int piercing = 0;
    // public scriptables
    public PoolApi pool;
	public PlayerScriptable playerInfo;

    //Max variables
    [SerializeField]
    private float speed_max, health_max, shoot_speed_max;

    public Sprite rifle, sniper, shotgun;
    public GameObject deadBody;


    //follow cam
    private GameObject following_camera;

  

	void Awake()
	{
        playerInfo.loc=transform;
		health=playerInfo.StartHealth;
		speed=playerInfo.StartSpeed;
        shoot_ps = GetComponentInChildren<ParticleSystem>();
        powerupAudio = GetComponent<AudioSource>();

        rb2d = GetComponent<Rigidbody2D>();

        bulletPickups = 1;

    }
	

	void Update () 
    {
        if( playerInfo.isalive )
        {
            aim(); // SHOOT IS CALLED HERE
            PlayerMove();
        }
    }


    public void Set_Camera( GameObject c )
    {
        following_camera = c;
    }


	void aim()
	{
		var child = transform.GetChild(1);

		Vector2 direction=new Vector2( Input.GetAxis( "RstickVertical" + playerInfo.playerNum ),
                                     Input.GetAxis( "RstickHorizontal" + playerInfo.playerNum ) );
		direction.Normalize();
		
        if(direction != Vector2.zero)
        {
            child.eulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            transform.GetChild(0).eulerAngles = child.eulerAngles + new Vector3(0, 0, -90f);
        }

        if (!shooting && Input.GetAxisRaw("shoot" + playerInfo.playerNum) >= .5)
	    	StartCoroutine(shoot(child.rotation));
	}

	void PlayerMove()
	{
		Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"+playerInfo.playerNum), 
                                        Input.GetAxisRaw("Vertical"+ playerInfo.playerNum));
        movement.Normalize();
        
        rb2d.MovePosition(movement * Time.deltaTime * speed + rb2d.position);
	}

	IEnumerator shoot(Quaternion angle)
	{
        shooting=true;
        shootAudio.pitch = Random.Range(0.9f, 1.1f);
        shootAudio.Play();
        StartCoroutine(ScreenShake(1 * damage));
        shoot_ps.Emit((int) (40 * damage));
        for(int i = 0; i < bullets_to_shoot; ++i)
        {
            var bullet = pool.RequestBullet("PlayerBullet" + playerInfo.playerNum);
            BulletMovement bm = bullet.GetComponent<BulletMovement>();
            bm.damage = damage;
            bm.speed = bulletSpeed;
            bm.piercing = piercing;
            bullet.GetComponent<BulletMovement>().damage = damage;
            if (bullet != null)
            {
                bullet.transform.position = shoot_ps.transform.position;
                bullet.transform.rotation = angle;
                bullet.transform.Rotate(0, 0, Random.Range(-spread, spread));
            }
        }


        yield return new WaitForSeconds(shoot_wait);

        shooting = false;
	}

    public void Take_Damage(float dam)
    {
        StartCoroutine(ScreenShake(5));
        if (!can_take_damage)
            return;
        if(!damage_wait)
            StartCoroutine(Invulnerability_Frames());

        health -= dam;

        if (health <= 0)
        {
            health = 0;
            playerInfo.isalive = false;
            gameObject.SetActive(false);
            Instantiate(deadBody, transform.position, Quaternion.identity);
        }

        //Play sounds for damage
    }


    void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Gun")
        {
            other.transform.GetChild(0).gameObject.SetActive(true);

            if (Input.GetAxisRaw("Pickup1") > 0)
            { 
                Gun_Properties gp = other.GetComponent<Gun_Properties>();

                if (gp.gunName == "Sniper")
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sniper;
                else if(gp.gunName == "Rifle")
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = rifle;
                else if(gp.gunName == "Shotgun")
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = shotgun;

                transform.GetChild(1).GetChild(0).localPosition = gp.laserLocation;

                shoot_wait = gp.reload;
                bullets_to_shoot = gp.bullets;
                spread = gp.spreadDegrees;
                shootAudio.clip = gp.shootSound;
                shoot_speed_max = gp.reload / 2f;
                damage = gp.damage;
                piercing = gp.piercing;
                bulletSpeed = gp.bulletSpeed;
                powerupAudio.clip = playerInfo.equipSound;
                powerupAudio.Play();
                Destroy(other.gameObject);
                bulletPickups = 1;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Gun")
        {
            other.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

  
    void OnTriggerEnter2D(Collider2D other)
    {

        if(other.tag == "SpeedUp")
        {
            powerupAudio.clip = playerInfo.speedSound;
            powerupAudio.Play();
            Destroy(other.gameObject);
            if(speed < speed_max)
                speed += 5;
        }
        else if(other.tag == "HealthUp")
        {
            powerupAudio.clip = playerInfo.healthSound;
            powerupAudio.Play();
            Destroy(other.gameObject);
            if (health < health_max)
                health += 1;
        }
        else if(other.tag == "ShootSpeedUp")
        {
            powerupAudio.clip = playerInfo.shootspeedSound;
            powerupAudio.Play();
            Destroy(other.gameObject);
            if (shoot_wait > shoot_speed_max)
            {
                shoot_wait -= 0.1f;
                if (bulletPickups < 3)
                {
                    bulletPickups++;
                }
            }
        }
        else if(other.tag == "Gun")
        {
            //This is here because it causes error because of the last else if
        }
        else if (playerInfo.fighting && other.tag == "PlayerBullet" + otherPlayerIndex)
        {
            BulletMovement bm = other.GetComponent<BulletMovement>();

            Take_Damage(bm.damage);
            if (bm.piercing-- == 0)
                bm.ReturnToPool();
        }
    }

    IEnumerator Invulnerability_Frames()
    {
        damage_wait = true;
        yield return new WaitForSeconds(0.1f);
        damage_wait = false;
        can_take_damage = false;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        for(int i = 0; i < 7; ++i)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.2f);
            yield return new WaitForSeconds(0.05f);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
            yield return new WaitForSeconds(0.05f);
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
        can_take_damage = true;
    }

    IEnumerator ScreenShake(float mult)
    {
        for (int i = 0; i < 5; ++i)
        {
            following_camera.transform.position += new Vector3(Random.Range(-.1f * mult, 0.1f * mult), Random.Range(-.1f * mult, .1f * mult));
            yield return new WaitForSeconds(0.05f);
        }
    }

    public float GetHealth()
    {
        return health;
    }

    public float GetSpeed()
    {
        return (speed-5)/5;
    }

    public float GetShootSpeed()
    {
        return bulletPickups;
    }
}
