using Assets.Scripts;
using UnityEngine;

public class ArrowShooterBehaviour : MonoBehaviour{

    private float reloadTimer;
    private Vector2 unitDirection;  //Unit direction that the arrow fires towards

    #region "Public interface"

    public int arrows = 1;  //The number of arrows in the trap

    public float force;                     //The amount of force to apply on the arrow
    public float reloadTime = 1f;   //The time between arrows

    public MapHelper.eDirection direction = MapHelper.eDirection.SOUTH; //Direction that the shooter is facing

    #endregion

    void Start ()
    {
        reloadTimer = 0;

        //TODO: Figure out how to configure the direction during map building
        //Pick a random direction
        direction = (MapHelper.eDirection) Random.Range((int)MapHelper.eDirection.NORTH, (int)MapHelper.eDirection.WEST + 1);

        unitDirection = MapHelper.UnitVectorFromDirection(direction);

        //Orient the shooter transform depending on the direction it should face
        OrientShooter();
    }

    // Update is called once per frame
    void Update () {
        if (arrows > 0)
        {
            //Decrement the reload timer if there is one
            if (reloadTimer > 0)
                reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0)
            {
                RaycastHit2D rc = Physics2D.Raycast(transform.position, unitDirection, float.PositiveInfinity,
                    1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Bears") | 1 << LayerMask.NameToLayer("Bunnies"));
                if ((rc.collider) && (rc.collider.tag == "Player") || (rc.collider.tag == "Bear") || (rc.collider.tag == "Bunny"))
                {
                    FireArrow();
                }
            }
        }
	}

    private void OrientShooter()
    {
        //Sprite by default is pointing south
        switch (direction)
        {
            case MapHelper.eDirection.NORTH:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case MapHelper.eDirection.WEST:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case MapHelper.eDirection.EAST:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MapHelper.eDirection.SOUTH:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }

    private void FireArrow()
    {
        //Build an arrow
        GameObject arrow = CreateArrow();

        //TODO: Configure the arrows direction

        //Place it on me
        arrow.transform.position = transform.position;

        //Fire away!
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.AddForce(unitDirection * force);

        //Deincrement number of arrows
        arrows--;

        //Reset the reload timer
        reloadTimer = reloadTime;
    }

    private GameObject CreateArrow()
    {
        GameObject arrow = Instantiate(MapBuilder.instance.arrow);

        //Orient the arrow based on the arrow shooter
        //By default the arrow sprite is pointing left
        switch (direction)
        {
            case MapHelper.eDirection.NORTH:
                arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case MapHelper.eDirection.WEST:
                arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MapHelper.eDirection.EAST:
                arrow.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case MapHelper.eDirection.SOUTH:
                arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }

        return arrow;
    }
}
