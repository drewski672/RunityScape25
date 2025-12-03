using Runity.Gameplay.Interactions;
using Runity.Gameplay.Player;
using Runity.Gameplay.UI;
using UnityEngine;

namespace Runity.Gameplay.Setup
{
    public class SandboxBootstrap : MonoBehaviour
    {
        [SerializeField] private Material groundMaterial;
        [SerializeField] private LayerMask groundLayer = 1 << 0;

        private void Start()
        {
            ContextMenuUI menu = new GameObject("ContextMenu").AddComponent<ContextMenuUI>();
            PlayerInteractor interactor;
            GameObject player = SpawnPlayer(menu, out interactor);
            Camera mainCamera = SetupCamera(player.transform);
            interactor.MainCamera = mainCamera;
            CreateGround();
            CreateTree();
            CreateDummy();
            CreateNeutralEnemy();
        }

        private GameObject SpawnPlayer(ContextMenuUI menu, out PlayerInteractor interactor)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);

            player.AddComponent<TickMover>();
            ClickToMove movement = player.AddComponent<ClickToMove>();
            interactor = player.AddComponent<PlayerInteractor>();
            interactor.ContextMenu = menu;
            interactor.GroundMask = groundLayer;

            player.AddComponent<TickHealth>();
            player.AddComponent<TickCombatant>();
            player.AddComponent<TickWoodcutter>();

            return player;
        }

        private Camera SetupCamera(Transform target)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            CameraOrbit orbit = mainCamera.gameObject.GetComponent<CameraOrbit>();
            orbit ??= mainCamera.gameObject.AddComponent<CameraOrbit>();
            orbit.SetTarget(target);

            return mainCamera;
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            ground.layer = LayerMaskToLayer(groundLayer);

            if (groundMaterial != null)
            {
                ground.GetComponent<MeshRenderer>().material = groundMaterial;
            }
        }

        private void CreateTree()
        {
            GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tree.name = "Tree";
            tree.transform.position = new Vector3(4f, 0.5f, 2f);
            tree.transform.localScale = new Vector3(1f, 1.5f, 1f);
            tree.AddComponent<TickHealth>();
            tree.AddComponent<TickTreeResource>();
            tree.AddComponent<ChoppableTree>();
        }

        private void CreateDummy()
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = "Training Dummy";
            dummy.transform.position = new Vector3(-4f, 1f, 2f);
            TickHealth health = dummy.AddComponent<TickHealth>();
            health.SetMaxHealth(10);
            dummy.AddComponent<TickCombatant>();
            dummy.AddComponent<DummyEnemy>();

            MeshRenderer renderer = dummy.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
        }

        private void CreateNeutralEnemy()
        {
            GameObject neutral = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            neutral.name = "Neutral Enemy";
            neutral.transform.position = new Vector3(-1f, 1f, -2f);
            neutral.AddComponent<TickHealth>();
            neutral.AddComponent<TickCombatant>();
            neutral.AddComponent<NeutralEnemy>();

            MeshRenderer renderer = neutral.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
        }

        private int LayerMaskToLayer(LayerMask mask)
        {
            int value = mask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
