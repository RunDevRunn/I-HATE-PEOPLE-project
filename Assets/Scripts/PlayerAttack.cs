using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerAttack : MonoBehaviour
{
    [Header("발사체 설정")]
    // 에셋 창에 있는 포탄 트리팹과 연결
    public GameObject projectilePrefab;
    /*
    // 포탄이 발사될 총구 위치 오브젝트와 연결
    // Transform 형식으로 선언한 이유: 총구의 위치와 방향 데이터가 담겨야 하는데 이런 위치, 회전, 크기 정보를 다루는 부품의 이름이 Transform
    // public Transform playerHandTransform;
    */
    public Transform firePoint;
    // 발사하는 힘의 세기 설정
    // Projectile.cs의 SetupProjectile() 함수에 전달될 값
    public float launchForce = 15f;


    [Header("연사 설정")]
    // 1. 연사 속도 설정
    public float fireRate = 0.1f;     
        // 2. 연사 쿨타임 계산용 변수
    private float fireCooldown = 0f;
        //3. 연사 상태 확인용 변수
    // 마우스를 누르고 있는 상태 
    private bool isFiring = false;     

    [Header("풀링 설정")]
    public int defaultPoolSize = 20; // 기본 풀 크기
    public int maxPoolSize = 100; // 최대 풀 크기


    // 이 무기가 발사하는 투사체 전용 풀 (다른 무기 프리팹과 섞이지 않음)
    private ObjectPool<Projectile> projectilePool;


    private Camera mainCamera;
    // mainCamera.ScreenToWorldPoint()의 반환 타입이 Vector3라서, worldMousePos도 Vector3로 선언해야 함.
    private Vector2 worldMousePos;
    // PlayerInput 컴포넌트 참조
    private PlayerInput playerInput;
    // "Attack" 액션에 대한 참조
    private InputAction attackActionRef;    


// 기존 코드에서 Upadte(), Shoot()에서 모두 Camera.main 호출해서 성능 안 좋았음. 이걸 캐싱(미리 올려두기)해서 보완. Awake()에서 Camera.main을 mainCamera에 저장하고, Update()와 Shoot()에서 mainCamera를 사용하도록 수정. == 캐싱
    void Awake()
    {
        mainCamera = Camera.main;

        playerInput = GetComponent<PlayerInput>();

        // "Attack" 액션의 실제 실행 중인 복사본을 코드로 바로 가져옴dw
        attackActionRef = playerInput.actions["Attack"];

        // 풀 생성 시점에 4가지 동작 콜백 등록(1.생성, 2.꺼낼 때, 3. 반납할 때, 4.파괴할 때)
        projectilePool = new ObjectPool<Projectile>(
            createFunc: CreateProjectile,
            actionOnGet: OnGetProjectile,
            actionOnRelease: OnReleaseProjectile,
            actionOnDestroy: OnDestroyProjectile,
            collectionCheck: true,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
    }

    void OnEnable()
    {
        // 액션 이벤트 구독
        attackActionRef.started += OnAttackStarted;
        attackActionRef.canceled += OnAttackCanceled;
    }

    void OnDisable()
    {
        // 액션 이벤트 구독 해제
        attackActionRef.started -= OnAttackStarted;
        attackActionRef.canceled -= OnAttackCanceled;
    }

    // 액션 이벤트 처리 메서드
    // ctx == context 줄임말
    void OnAttackStarted(InputAction.CallbackContext ctx)
    {
        isFiring = true;
    }

    void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        isFiring = false;
    }

    // Projectile 클래스를 의미하는게 아님. 콜백함수 createFunc: CreateProjectile를 구현한 것. ObjectPool<Projectile>이 요구하는 반환타입이 Projectile인 delegate 함수
    Projectile CreateProjectile()
    {
        GameObject obj = Instantiate(projectilePrefab);

        // Projectile은 반환타입을 의미하는게 아님. Projectile.cs 스크립트가 붙어있는 오브젝트를 의미함. Projectile.cs 스크립트가 붙어있지 않으면 GetComponent<Projectile>()에서 null 반환됨
        Projectile projectile = obj.GetComponent<Projectile>();

        // 투사체가 스스로 반납을 요청할 수 있도록 풀 참조를 넘겨줌
        projectile.SetPool(projectilePool);

        return projectile;
    }

    // Get() 호출 시 매번 실행(풀에서 꺼낼 때 상태 초기화)
    void OnGetProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    // Release() 호출 시 매번 실행(풀로 되돌아갈 때 상태 정리)
    void OnReleaseProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    // 풀 용량 초과로 파괴될 때만 실행
    void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
    }


    void Shoot()
    {
        // 예외 처리: 프리팹이나 총구 위치가 지정 안 되었으면 실행 안함
        if (projectilePrefab == null) 
        { Debug.LogError("프리팹이 비었습니다!"); return; }
        if (firePoint == null) 
        { Debug.LogError("firePoint가 비었습니다!"); return; }
        Debug.Log("2. 예외 처리 통과, 포탄 생성 직전!");

        // Instantiate() 대신 풀에서 재사용 가능한 투사체를 꺼내옴
        Projectile projectile = projectilePool.Get();

        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = Quaternion.identity;


        /*
        // [역할 2] 마우스 화면 좌표를 게임 월드 좌표로 변환 & Z축 보정
        // 제대로 보정된 targetMousePos를 사용하여 월드 좌표로 변환
        // ★ [핵심 수정] Update()에서 검증된 방식과 똑같이 유니티 순정 실시간 마우스 좌표를 가져옵니다!
        // shootDireciton 을 firePoint.right로 계산 가능해서, 마우스 좌표를 굳이 쓸 필요가 없어서 주석 처리함.
        
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // [역할 3] 총구와 마우스 위치 사이의 '방향' 계산
        // firePoint의 오른쪽 방향을 기준으로 발사 방향 계산
        // Update()에서 이미 방향 계산 끝남.firePoint.rotataion == lookDirection 
        */
        Vector2 shootDirection = firePoint.right;  

        // 이전 발사 상태(속도 등)가 남지 않도록 내부에서 완전히 초기화됨
        projectile.SetupProjectile(shootDirection, launchForce);
        /*
        // [역할 4] 생성된 포탄에게 방향과 힘을 전달하며 "날아가!"라고 명령
        // Projectile.cs의 SetupProjectile() 함수 호출
        // Projectile 써준 이유. 내가 지금부터 Projectile이라는 이름의 스크립트를 만질거야 라고 데이터 종류 선언
        // projectileLogic이라는 변수에 projectile 오브젝트에 붙어있는 Projectile.cs 스크립트의 데이터를 가져와서 연결
        */
        /*
        Projectile projectileLogic = projectile.GetComponent<Projectile>();
        if (projectileLogic != null)
        {
            projectileLogic.SetupProjectile(shootDirection, launchForce);
        }
        */
    }

// 마우스 조준
    void Update()
    {
        // 레거시 Input.mousePosition 대신 New Input System의 Mouse.current.position.ReadValue()를 사용하여 마우스 화면 좌표를 가져옴
        Vector2 screenMousePos = Mouse.current.position.ReadValue();

        worldMousePos = mainCamera.ScreenToWorldPoint(screenMousePos);
        
        /* 레거시 input System에서 마우스 좌표를 가져오는 방식
        mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        */
        /*
        1. firePoint의 위치를 플레이어 자식인 Hand의 위치로 동기화
        firePoint.position = firePoint.position;

        2. 마우스를 바라보는 월드 방향 벡터 구하기 (이제 firePoint 기준으로 방향 계산)
        mouseWorldPos가 다른 메서드에서 필요해지는 순간이 올 수 있기 때문에 클래스 변수로 선언해서 구조 수정
        

        (Vector2) 캐스팅. 새 오브젝트 생성 없이 z축만 무시함.
        Vector2 lookDirecton = new Vector2 형식으로 쓰면 새 오브젝트를 메모리에 생성하게 된다
        */ 
        Vector2 lookDirection = worldMousePos - (Vector2)firePoint.position;

        // 3. ★핵심 수정★ transform.rotation(X) -> firePoint.rotation(O)
        // 나(Player)를 돌리지 말고, 변수로 가져온 firePoint를 돌려라!
        firePoint.rotation = Quaternion.FromToRotation(Vector2.right, lookDirection);

        //연사 알고리즘
        // 쿨타임이 아직 남아있으면 0될때까지 계속 감소시킨다
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        // 발사중 상태이고 쿨타임이 0이거나 0보다 작으면
        if (isFiring && fireCooldown <= 0f)
        {
            Shoot();
            // 0이 된 쿨타임을 다시 fireRate로 초기화함
            fireCooldown = fireRate;
        }
    }


}


