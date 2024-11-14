using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;     // Скорость передвижения
    public float upSpeed = 4f;       // Скорость движения вверх
    public GameObject projectilePrefab;  // Префаб снаряда для стрельбы
    public Transform firePoint;      // Точка стрельбы

    // Новые переменные
    public float dashSpeed = 10f;    // Скорость рывка
    public float slideSpeed = 2f;    // Скорость скольжения по стене
    public float parachuteFallSpeed = 2f;  // Скорость падения с парашютом
    private bool isParachuteActive = false;  // Активирован ли парашют
    private bool isDashing = false;  // Происходит ли рывок
    private bool isSliding = false;  // Скольжение по стене
    private bool isInvincible = false;  // Неуязвимость

    private Rigidbody2D rb;
    
    private Camera mainCamera;
    private bool isTeleportMode = false;  // Включен ли режим телепортации

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        // Зафиксировать вращение персонажа на оси Z
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Движение влево/вправо
        float moveInput = Input.GetAxis("Horizontal");
        
        if (!isDashing)  // Если не происходит рывок
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        // Движение вверх
        if (Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, upSpeed);
        }

        // Стрельба
        if (Input.GetKeyDown(KeyCode.F))
        {
            FireProjectile();
        }

        // Включение/выключение режима телепортации
        if (Input.GetKeyDown(KeyCode.T))
        {
            isTeleportMode = !isTeleportMode;  // Переключаем режим телепортации
            Debug.Log("Teleport Mode: " + (isTeleportMode ? "Enabled" : "Disabled"));
        }

        // Если режим телепортации включен, ждем клика мышкой
        if (isTeleportMode && Input.GetMouseButtonDown(0))
        {
            TeleportToMousePosition();
        }

        // Камера следует за персонажем
        FollowCamera();

        // Рывок вперед
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        // Активация парашюта для замедления падения
        if (Input.GetKey(KeyCode.Space))
        {
            isParachuteActive = true;
        }
        else
        {
            isParachuteActive = false;
        }

        // Скользить по стенам
        if (isSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -slideSpeed);
        }

        // Применение парашюта при падении
        if (isParachuteActive && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -parachuteFallSpeed);
        }
    }

    // Стрельба снарядами
    void FireProjectile()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    // Телепортация по клику мышки
    void TeleportToMousePosition()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition;  // Перемещаем персонажа в точку клика
    }

    // Камера следует за персонажем
    void FollowCamera()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.y;
        mainCamera.transform.position = cameraPosition;
    }

    // Рывок вперед
    IEnumerator Dash()
    {
        isDashing = true;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.2f);  // Рывок длится 0.2 секунды
        isDashing = false;
    }

    // Неуязвимость
    IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        // Например, можно изменить цвет персонажа
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(3f);  // 3 секунды неуязвимости
        GetComponent<SpriteRenderer>().color = Color.white;
        isInvincible = false;
    }

    // Проверка на скольжение по стенам
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isSliding = true;
        }

        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isSliding = false;
        }
    }
}
