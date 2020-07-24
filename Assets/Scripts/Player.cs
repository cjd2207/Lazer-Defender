using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	// config params
    [Header("Player")]
	[SerializeField] float moveSpeed = 10f;
	[SerializeField] float padding = 1f;
    [SerializeField] int health = 200;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] [Range(0, 1)] float deathSoundVolume = 0.7f;

    [Header("Projectile")]
	[SerializeField] GameObject laserPrefab;
	[SerializeField] float projectileSpeed = 10f;
	[SerializeField] float projectileFiringPeriod = 0.1f;
    [SerializeField] AudioClip projectileSFX;
    [SerializeField] [Range(0, 1)] float projectileSoundVolume = 0.7f;

    Coroutine firingCorountine;

	float xMin, xMax;
	float yMin, yMax;

	// Use this for initialization
	void Start () 
	{
		SetUpMoveBoundaries ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Move ();
		Fire ();
	}

	private void Move()
	{
		var deltaX = Input.GetAxis ("Horizontal") * Time.deltaTime * moveSpeed;
		var deltaY = Input.GetAxis ("Vertical") * Time.deltaTime * moveSpeed;

		var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
		var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);

		transform.position = new Vector2 (newXPos, newYPos);
	}

	private void Fire()
	{
		if (Input.GetButtonDown ("Fire1")) {
			firingCorountine = StartCoroutine (FireContinuously ());
		}
		if (Input.GetButtonUp ("Fire1")) {
			StopCoroutine (firingCorountine);
		}
	}

	IEnumerator FireContinuously()
	{
		while (true) {
			GameObject laser = Instantiate (laserPrefab, transform.position, Quaternion.identity) as GameObject;
			laser.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0, projectileSpeed);
            AudioSource.PlayClipAtPoint(projectileSFX, Camera.main.transform.position, projectileSoundVolume);
            yield return new WaitForSeconds (projectileFiringPeriod);
		}
	}

	private void SetUpMoveBoundaries()
	{
		Camera gameCamrera = Camera.main;

		xMin = gameCamrera.ViewportToWorldPoint (new Vector3 (0, 0, 0)).x + padding;
		xMax = gameCamrera.ViewportToWorldPoint (new Vector3 (1, 0, 0)).x - padding;

		yMin = gameCamrera.ViewportToWorldPoint (new Vector3 (0, 0, 0)).y + padding;
		yMax = gameCamrera.ViewportToWorldPoint (new Vector3 (0, 1, 0)).y - padding;

	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) return;
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        if (health <= 0) health = 0;
        damageDealer.Hit();

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        FindObjectOfType<Level>().LoadGameOver();
        Destroy(gameObject);
        AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position, deathSoundVolume);
    }

    public int GetHealth()
    {
        return health;
    }
}
