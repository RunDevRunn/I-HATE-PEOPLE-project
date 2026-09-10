using UnityEngine;
// new 입력시스템 작동시키려면 필요한 네임스페이스
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{

public float speed = 5f;
public float jumpForce = 20f;
Vector2 inputVec;
// 물리 엔진의 육체 추가 (변수 선언)
Rigidbody2D rigid;


[Header("2단 점프 설정")]
// 바닥 체크 기준점
// Transform형을 쓰는 이유: 자식 오브젝트로 캐릭터를 따라다니게 하기 위해
public Transform groundCheck;
// 바닥 감지 원의 반지름
public float groundCheckRadius = 0.2f;

// 'Ground' 레이어만 감지하도록 필터링
public LayerMask groundLayer;
// 현재 땅에 닿아 있는지 체크
bool isGrounded;

// 최대 점프 가능 횟수 (2단 점프: 2회)
public int maxJumpCount = 2;
// 현재 남은 점프 가능 횟수
int jumpCountLeft;



void Awake()
{
        // 물리 엔진 변수 초기화
        // GetComponent<>()는 비용이 꽤 비싼 함수입니다. 지금처럼 Awake()에서 한 번만 캐싱하는 패턴을 모든 컴포넌트에 습관화하세요. Update()나 FixedUpdate() 안에서 호출하면 매 프레임마다 탐색해서 성능이 크게 떨어집니다.
        rigid = GetComponent<Rigidbody2D>();
}

// New Input System에서 키보드 입력을 감지하면 Move 액션이 호출되는 함수

/* 입력값의 종류는 다양함(wasd이동 -> Vector2(x,y), 점프 -> float(0 or 1), 마우스 위치 -> Vector2)
그래서 InputValue라는 범용 컨테이너를 만들고, Get<T>()라는 제네릭 함수를 통해서 원하는 타입으로 변환해서 사용합니다. */

/*💡 개발 팁
1. 타입 불일치 주의(중요): Input Actions에서 설정한 Action Type(Value/Button/Pass Through)과 Control Type이 실제 코드의 Get<T>() 타입과 맞아야 해요. 안 맞으면 런타임 에러가 납니다.

2. 간단하게 확인하는 법: Input Actions 에셋(.inputactions)을 열어서 해당 액션을 클릭하면 우측에 "Control Type"이 표시돼요. 거기 적힌 타입을 그대로 Get<>()에 넣으면 됩니다.

3. 버튼은 float으로도, bool처럼도 쓸 수 있음: value.Get<float>() > 0.5f로 눌림 여부를 판단하는 패턴도 자주 씁니다.*/
void OnMove(InputValue value)
{
    // WASD에 따라 (-1~1, -1~1) 범위의 벡터값이 들어옴
    // Action Type과 Control Type이 실제 코드의 Get<T>() 타입과 맞아야 해요
    inputVec = value.Get<Vector2>();
}

void OnJump(InputValue value)
{
    // 버튼이 눌렸고 && 땅에 닿아 있을 때만 점프 허용
    if (value.isPressed && jumpCountLeft > 0)
    {
        // 기존 y축 속도를 0으로 초기화 후 점프력 부여
        // 연속 점프 시 힘이 누적되는 걸 방지하기 위해, 점프 직전 y축 속도를 0으로 초기화하고 점프력만 부여합니다.
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);

        rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // 점프 한 번 사용한 만큼 카운트 차감
        jumpCountLeft--;
    }        
}


    // 입력받은 데이터 바탕으로 '이동' 시켜주는 물리엔진 가동
    // ⚠️ New Input System의 Vector2 액션은 자동 normalize 옵션이 있기도 하지만, 코드에서 명시적으로 처리하는 게 더 안전합니다.
void FixedUpdate()
{
    
    /* 260714 코드 추가 - 2단 점프 제한
    groundCheck 기준점에서 groundCheckRadius 반지름만큼의 원을 그려서, 그 안에 'Ground' 레이어가 있는지 체크

    💡 왜 Collision 이벤트(OnCollisionEnter2D) 대신 OverlapCircle을 쓰나요?
    OnCollisionEnter2D/OnCollisionExit2D로 isGrounded = true/false를 토글하는 방식도 흔한데, 벽에 살짝 스치거나 여러 콜라이더가 겹칠 때 Exit이 씹혀서 isGrounded가 잘못된 상태로 고정되는 버그가 자주 생겨요. OverlapCircle은 매 프레임 "지금 이 순간" 상태를 다시 계산하니 훨씬 안정적입니다.

    이전 프레임의 값을 wasGrounded에 저장해두고, 현재 프레임의 isGrounded와 비교해서 '착지한 순간'을 감지합니다.
    isGrounded를 새로 계산하기 직전 (이전 프레임의 값 대입)
    */
    bool wasGrounded = isGrounded;
    
    // isGrounded 값 새로 갱신 == 현재 프레임의 값
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    /* 공중에 있다가 '착지한 순간'에만 카운트 리셋
    !wasGrounde : wasGrounded가 false였다 == 공중에 있었다
    직전 프레임 공중 && 현재 프레임 땅 ==> 착지한 순간
    */
    if (!wasGrounded && isGrounded)
    {
        // 땅에 닿은 순간, 점프 횟수 초기화
        jumpCountLeft = maxJumpCount;
    }
    /* 260702 코드 수정
    // Q. 시간은 왜 곱해 주는 거지? -> 프레임마다 이동량이 달라지지 않게 하기 위해서
    // Q. 계산한 값은 벡터야 스칼라야? -> 벡터
    // Q. 속도와 시간이 스칼라 값이니까? -> 맞아요. 속도와 시간은 스칼라 값이니까, 벡터에 곱하면 벡터가 됨
    Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
    rigid.MovePosition(rigid.position + nextVec);
    */

    /* MovePosition 대신 linearVelocity를 직접 제어하는 방식으로 변경
    x는 입력값 기준으로 매 프레임 덮어씀, y는 그대로 유지 -> y속도 중력과 자연스럽게 공존
    */
    rigid.linearVelocity = new Vector2(inputVec.x * speed, rigid.linearVelocity.y);
}

}