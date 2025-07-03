using UnityEngine;

/// <summary>
/// �G�̏��enum��
/// </summary>
public enum EnemyState
{
    IDLE,
    PATROL,
    CHASE,
    ATTACK,
    ESCAPE,
    DEAD
}

/// <summary>
/// State�}�V����Unity�g�p��
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    [Header("�ݒ�")]
    [SerializeField] private float detectionRange_ = 10f;
    [SerializeField] private float attackRange_ = 2f;
    [SerializeField] private float escapeHealthPercent_ = 0.3f;
    [SerializeField] private float currentHealth_ = 100f;
    [SerializeField] private float maxHealth_ = 100f;

    [Header("�f�o�b�O")]
    [SerializeField] private EnemyState currentState_;

    // �X�e�[�g�}�V��
    private StateMachine<EnemyState> stateMachine_;
    
    // �v���C���[�Q�Ɓi��j
    private Transform player_;
    private bool playerInRange_;

    void Start()
    {
        // �v���C���[��T���i��j
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player_ = playerObj.transform;

        // �X�e�[�g�}�V���̏�����
        InitializeStateMachine();
        
        // ������Ԃ�IDLE�ɐݒ�
        stateMachine_.Start(EnemyState.IDLE);
    }

    void Update()
    {
        // �v���C���[�Ƃ̋����`�F�b�N
        UpdatePlayerDetection();
        
        // �X�e�[�g�}�V���̍X�V
        stateMachine_.UpdateCurrent();
        
        // �f�o�b�O�p�F���݂̏�Ԃ�\��
        currentState_ = stateMachine_.CurrentState;
    }

    /// <summary>
    /// �X�e�[�g�}�V����������
    /// </summary>
    private void InitializeStateMachine()
    {
        stateMachine_ = new StateMachine<EnemyState>();

        // IDLE��Ԃ̐ݒ�
        stateMachine_.RegisterState(EnemyState.IDLE)
            .SetCallbacks(
                onEntry: () => Debug.Log("�G�F�ҋ@�J�n"),
                onUpdate: () => { /* �ҋ@���̏��� */ },
                onExit: () => Debug.Log("�G�F�ҋ@�I��")
            )
            .AddTransition(EnemyState.PATROL, () => !playerInRange_)
            .AddTransition(EnemyState.CHASE, () => playerInRange_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // PATROL��Ԃ̐ݒ�
        stateMachine_.RegisterState(EnemyState.PATROL)
            .SetCallbacks(
                onEntry: () => Debug.Log("�G�F����J�n"),
                onUpdate: () => DoPatrol(),
                onExit: () => Debug.Log("�G�F����I��")
            )
            .AddTransition(EnemyState.CHASE, () => playerInRange_)
            .AddTransition(EnemyState.IDLE, () => Random.Range(0f, 1f) < 0.01f) // 1%�̊m���őҋ@
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // CHASE��Ԃ̐ݒ�
        stateMachine_.RegisterState(EnemyState.CHASE)
            .SetCallbacks(
                onEntry: () => Debug.Log("�G�F�ǐՊJ�n"),
                onUpdate: () => DoChase(),
                onExit: () => Debug.Log("�G�F�ǐՏI��")
            )
            .AddTransition(EnemyState.ATTACK, () => GetDistanceToPlayer() <= attackRange_)
            .AddTransition(EnemyState.ESCAPE, () => GetHealthPercent() <= escapeHealthPercent_)
            .AddTransition(EnemyState.PATROL, () => !playerInRange_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // ATTACK��Ԃ̐ݒ�
        stateMachine_.RegisterState(EnemyState.ATTACK)
            .SetCallbacks(
                onEntry: () => Debug.Log("�G�F�U���J�n"),
                onUpdate: () => DoAttack(),
                onExit: () => Debug.Log("�G�F�U���I��")
            )
            .AddTransition(EnemyState.CHASE, () => GetDistanceToPlayer() > attackRange_)
            .AddTransition(EnemyState.ESCAPE, () => GetHealthPercent() <= escapeHealthPercent_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // ESCAPE��Ԃ̐ݒ�
        stateMachine_.RegisterState(EnemyState.ESCAPE)
            .SetCallbacks(
                onEntry: () => Debug.Log("�G�F�����J�n"),
                onUpdate: () => DoEscape(),
                onExit: () => Debug.Log("�G�F�����I��")
            )
            .AddTransition(EnemyState.CHASE, () => GetHealthPercent() > escapeHealthPercent_ && playerInRange_)
            .AddTransition(EnemyState.PATROL, () => !playerInRange_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // DEAD��Ԃ̐ݒ�
        stateMachine_.RegisterState(EnemyState.DEAD)
            .SetCallbacks(
                onEntry: () => {
                    Debug.Log("�G�F���S");
                    // ���S����
                    gameObject.SetActive(false);
                },
                onUpdate: () => { /* ���S��Ԃł͉������Ȃ� */ },
                onExit: () => { /* ���S��Ԃ���͏o�Ȃ� */ }
            );
    }

    #region ��ԕʂ̏���

    private void DoPatrol()
    {
        // ���񏈗�
        // transform.Translate(Vector3.forward * Time.deltaTime);
    }

    private void DoChase()
    {
        // �ǐՏ���
        if (player_ != null)
        {
            Vector3 direction = (player_.position - transform.position).normalized;
            transform.Translate(direction * Time.deltaTime * 3f);
        }
    }

    private void DoAttack()
    {
        // �U������
        Debug.Log("�U����...");
    }

    private void DoEscape()
    {
        // ��������
        if (player_ != null)
        {
            Vector3 direction = (transform.position - player_.position).normalized;
            transform.Translate(direction * Time.deltaTime * 5f);
        }
    }

    #endregion

    #region �w���p�[���\�b�h

    private void UpdatePlayerDetection()
    {
        if (player_ == null) return;
        
        float distance = GetDistanceToPlayer();
        playerInRange_ = distance <= detectionRange_;
    }

    private float GetDistanceToPlayer()
    {
        if (player_ == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player_.position);
    }

    private float GetHealthPercent()
    {
        return currentHealth_ / maxHealth_;
    }

    #endregion

    #region ���J���\�b�h

    /// <summary>
    /// �_���[�W���󂯂�
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth_ = Mathf.Max(0, currentHealth_ - damage);
    }

    /// <summary>
    /// �����I�ɏ�Ԃ�ύX
    /// </summary>
    public void ForceState(EnemyState newState)
    {
        stateMachine_.TransitionTo(newState);
    }

    #endregion

    #region �f�o�b�O

    void OnDrawGizmosSelected()
    {
        // ���o�͈͂̕\��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange_);
        
        // �U���͈͂̕\��
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange_);
    }

    #endregion
}