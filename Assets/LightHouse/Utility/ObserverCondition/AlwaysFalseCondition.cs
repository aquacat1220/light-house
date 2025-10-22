using UnityEngine;
using FishNet.Observing;
using FishNet.Connection;

[CreateAssetMenu(fileName = "AlwaysFalseCondition", menuName = "Scriptable Objects/AlwaysFalseCondition")]
public class AlwaysFalseCondition : ObserverCondition
{
    /// <summary>
    /// Returns if the object which this condition resides should be visible to connection.
    /// </summary>
    /// <param name="connection">Connection which the condition is being checked for.</param>
    /// <param name="currentlyAdded">True if the connection currently has visibility of this object.</param>
    /// <param name="notProcessed">True if the condition was not processed. This can be used to skip processing for performance. While output as true this condition result assumes the previous ConditionMet value.</param>
    public override bool ConditionMet(NetworkConnection connection, bool currentlyAdded, out bool notProcessed)
    {
        notProcessed = false;
        return false;
    }

    /// <summary>
    /// Type of condition this is. Certain types are handled different, such as Timed which are checked for changes at timed intervals.
    /// </summary>
    /// <returns></returns>
    /* Since clientId does not change a normal condition type will work.
    * See API on ObserverConditionType for more information on what each
    * type does. */
    public override ObserverConditionType GetConditionType() => ObserverConditionType.Normal;
}
