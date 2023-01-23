namespace MTCG_Tests
{
    public class CardTests
    {
        [Test, Order(1)]
        public void CreateCards_FailureSize()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Payload = "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, " +
                "{\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", " +
                "\"Name\":\"WaterSpell\", \"Damage\": 20.0}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}]";
            var card = new Card();
            card.CreateCards(mockEventArgs.Object);
            mockEventArgs.Verify(e => e.Reply(400, "Error occured while creating package: not enough cards for a package."));
        }

        [Test, Order(2)]
        public void CreateCards_SuccessSize()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Payload = "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, " +
                "{\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", " +
                "\"Name\":\"WaterSpell\", \"Damage\": 20.0}, " +
                "{\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}, {\"Id\":\"9e8238a4-8a7a-487f-9f7d-a8c97899eb48\", \"Name\":\"Dragon\", \"Damage\": 70.0},]";
            var card = new Card();
            card.CreateCards(mockEventArgs.Object);
            mockEventArgs.Verify(e => e.Reply(400, "Error occured while creating package: not enough cards for a package."), Times.Never());
        }

        [Test, Order(3)]
        public void GetCardStats_SuccessSpell()
        {
            Card card = new Card();
            card.Name = "FireSpell";
            card.GetCardStats(card);
            Assert.That(card.Element, Is.EqualTo(Card.CardElement.Fire));
            Assert.That(card.Type, Is.EqualTo(Card.CardType.Spell));
        }

        [Test, Order(4)]
        public void GetCardStats_SuccessMonster() { 
            Card card2 = new Card();
            card2.Name = "Kraken";
            card2.GetCardStats(card2);
            Assert.That(card2.Element, Is.EqualTo(Card.CardElement.Regular));
            Assert.That(card2.Type, Is.EqualTo(Card.CardType.Kraken));
        }

        [Test, Order(5)]
        public void GetCardStatsFailure()
        {
            Card card = new Card();
            card.Name = "DarkSpell";

            Assert.That(() => card.GetCardStats(card), Throws.TypeOf<ArgumentException>());
        }
    }
}