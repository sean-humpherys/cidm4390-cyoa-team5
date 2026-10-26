// How the game ended (End Game Activity Diagram). Passed to Game.EndGame().
public enum EndCondition
{
    Retreat,          // retreated when first meeting the dragon  -> message 400
    DragonDefeated,   // player won the fight                     -> message 401
    PlayerDefeated,   // player lost the fight                    -> message 402
    Quit              // typed 'e' on the adventure menu          -> message 5
}
