namespace MTCG_Tests
{
    public class BattleTests
    {
        // special interaction should set the correct winner and result string
        [Test, Order(1)]
        public void SpecialInteraction_p1DragonDefeatsGoblin()
        {
            Card cardOne = new Card();
            cardOne.Type = Card.CardType.Dragon;
            Card cardTwo = new Card();
            cardTwo.Type = Card.CardType.Goblin;
            Battle battle = new Battle(null, null);

            string result = battle.SpecialInteraction(cardOne, cardTwo);
            Assert.That(result, Is.EqualTo("Dragon defeats Goblin\n"));
            Assert.That(battle.PlayerOneWins, Is.EqualTo(true));
        }

        // special interaction should set the correct winner and result string
        [Test, Order(2)]
        public void SpecialInteraction_p2WaterSpellDrownsKnight()
        {
            Card cardOne = new Card();
            cardOne.Type = Card.CardType.Knight;
            Card cardTwo = new Card();
            cardTwo.Type = Card.CardType.Spell;
            cardTwo.Element = Card.CardElement.Water;
            Battle battle = new Battle(null, null);

            string result = battle.SpecialInteraction(cardOne, cardTwo);
            Assert.That(result, Is.EqualTo("WaterSpell drowns Knight\n"));
            Assert.That(battle.PlayerOneWins, Is.EqualTo(false));
        }

        //  no special interaction should return an empty string
        [Test, Order(3)]
        public void SpecialInteraction_OrkVsOrk()
        {
            Card cardOne = new Card();
            cardOne.Type = Card.CardType.Ork;
            Card cardTwo = new Card();
            cardTwo.Type = Card.CardType.Ork;
            Battle battle = new Battle(null, null);

            string result = battle.SpecialInteraction(cardOne, cardTwo);
            Assert.That(result, Is.EqualTo(""));
        }

        //  card of the losing player should be transfered to the deck of the winning player
        [Test, Order(4)]
        public void ChangeAfterRound_p1SuccessfulCardTransfer()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            Card cardTwo = new Card();

            battle.DeckOne.Add(cardOne);
            battle.DeckTwo.Add(cardTwo);
            battle.PlayerOneWins = true;

            battle.ChangeCardAfterRound(0, 0);

            Assert.That(battle.DeckOne.Count(), Is.EqualTo(2));
            Assert.That(battle.DeckTwo.Count(), Is.EqualTo(0));
        }

        //  card of the losing player should be transfered to the deck of the winning player
        [Test, Order(5)]
        public void ChangeAfterRound_p2SuccessfulCardTransfer()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            Card cardTwo = new Card();

            battle.DeckOne.Add(cardOne);
            battle.DeckTwo.Add(cardTwo);
            battle.PlayerOneWins = false;

            battle.ChangeCardAfterRound(0, 0);

            Assert.That(battle.DeckOne.Count(), Is.EqualTo(0));
            Assert.That(battle.DeckTwo.Count(), Is.EqualTo(2));
        }

        //  spell vs spell element damage should double/halve
        [Test, Order(6)]
        public void CalculateDamage_WaterSpellVsFireSpell()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            cardOne.Name = "WaterSpell";
            cardOne.Damage = 20F;

            Card cardTwo = new Card();
            cardTwo.Name = "FireSpell";
            cardTwo.Damage = 20F;

            float[] calcDamage = battle.CalculateDamage(cardOne, cardTwo);

            Assert.That(calcDamage[0], Is.EqualTo(40F));
            Assert.That(calcDamage[1], Is.EqualTo(10F));
        }

        //  monster vs monster element damage should remain unchanged 
        [Test, Order(7)]
        public void CalculateDamage_WaterGoblinVsFireGoblin()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            cardOne.Name = "WaterGoblin";
            cardOne.Damage = 20F;

            Card cardTwo = new Card();
            cardTwo.Name = "FireGoblin";
            cardTwo.Damage = 20F;

            float[] calcDamage = battle.CalculateDamage(cardOne, cardTwo);

            Assert.That(calcDamage[0], Is.EqualTo(20F));
            Assert.That(calcDamage[1], Is.EqualTo(20F));
        }

        //  mixed fight element damage should double/halve
        [Test, Order(8)]
        public void CalculateDamage_WaterGoblinVsFireSpell()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            cardOne.Name = "WaterGoblin";
            cardOne.Damage = 20F;

            Card cardTwo = new Card();
            cardTwo.Name = "FireSpell";
            cardTwo.Damage = 20F;

            float[] calcDamage = battle.CalculateDamage(cardOne, cardTwo);

            Assert.That(calcDamage[0], Is.EqualTo(40F));
            Assert.That(calcDamage[1], Is.EqualTo(10F));
        }

        //  mixed fight same element should stay the same
        [Test, Order(9)]
        public void CalculateDamage_WaterGoblinVsWaterSpell()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            cardOne.Name = "WaterGoblin";
            cardOne.Damage = 20F;

            Card cardTwo = new Card();
            cardTwo.Name = "WaterSpell";
            cardTwo.Damage = 20F;

            float[] calcDamage = battle.CalculateDamage(cardOne, cardTwo);

            Assert.That(calcDamage[0], Is.EqualTo(20F));
            Assert.That(calcDamage[1], Is.EqualTo(20F));
        }

        // winning player should get a dmg boost of 11 and losing player loses his card
        [Test, Order(10)]
        public void RemoveCardAfterRoundAndBoostDmg_p1Wins()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            cardOne.Damage = 20.0F;
            Card cardTwo = new Card();
            cardTwo.Damage = 10.0F;

            battle.DeckOne.Add(cardOne);
            battle.DeckTwo.Add(cardTwo);
            battle.PlayerOneWins = true;

            battle.RemoveCardAfterRoundAndBoostDmg(0, 0);

            Assert.That(battle.DeckOne.Count(), Is.EqualTo(1));
            Assert.That(battle.DeckOne[0].Damage, Is.EqualTo(31.0F));
            Assert.That(battle.DeckTwo.Count(), Is.EqualTo(0));
        }
        // winning player should get a dmg boost of 11 and losing player loses his card
        [Test, Order(11)]
        public void RemoveCardAfterRoundAndBoostDmg_p2Wins()
        {
            Battle battle = new Battle(null, null);
            Card cardOne = new Card();
            cardOne.Damage = 20.0F;
            Card cardTwo = new Card();
            cardTwo.Damage = 10.0F;

            battle.DeckOne.Add(cardOne);
            battle.DeckTwo.Add(cardTwo);
            battle.PlayerOneWins = false;

            battle.RemoveCardAfterRoundAndBoostDmg(0, 0);

            Assert.That(battle.DeckTwo.Count(), Is.EqualTo(1));
            Assert.That(battle.DeckTwo[0].Damage, Is.EqualTo(21.0F));
            Assert.That(battle.DeckOne.Count(), Is.EqualTo(0));
        }
    }
}