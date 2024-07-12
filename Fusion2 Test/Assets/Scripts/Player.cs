using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private MeshRenderer[] playerModelParts;
    [SerializeField] private float sens;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpulse = 10.0f;

    [Networked] public string Name { get; set; }
    [Networked] private NetworkButtons previousButtons { get; set; }

    public bool isReady;

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority)
        {
            foreach (MeshRenderer rend in playerModelParts)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            Runner.GetComponent<InputManager>().LocalPlayer = this;
            CameraFollow.Instance.SetTarget(cameraTarget);
            Name = PlayerPrefs.GetString("Photon.Menu.Username");
            RPC_PlayerName(Name);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name)
    {
        Name = name;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            kcc.AddLookRotation(sens * input.LookDelta);
            Vector3 worldDir = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
            UpdateCamTarget();
            float jump = 0f;
            if (input.Buttons.WasPressed(previousButtons, InputButton.Jump) && kcc.IsGrounded)
            {
                jump = jumpImpulse;
            }

            kcc.Move(worldDir.normalized * speed, jump);
            previousButtons = input.Buttons;

        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        isReady = true;
        UIManager.Instance.SetReady();
    }

    public override void Render()
    {
        UpdateCamTarget();
    }

    private void UpdateCamTarget()
    {
        cameraTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        kcc.SetPosition(position);
        kcc.SetLookRotation(rotation);
    }
}
