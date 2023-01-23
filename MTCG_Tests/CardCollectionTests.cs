namespace MTCG_Tests
{
    public class CardCollectionTests
    {
        //  less than 3 cards in an update request should result in a Malformed Request Reply
        [Test, Order(1)]
        public void UpdateDeck_FailureSize()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Payload = "[\"aa9999a0-734c-49c6-8f4a-651864b14e62\", \"d6e9c720-9b5a-40c7-a6b2-bc34752e3463\", \"d60e23cf-2238-4d49-844f-c7589ee5342e\"]";
            UserToken userToken = new UserToken();
            CardCollection cardCollection = new CardCollection();
            cardCollection.UpdateDeck(mockEventArgs.Object, userToken);
            mockEventArgs.Verify(e => e.Reply(400, "Malformed Request to update Decks."));
        }

        //  4 cards in an update request shouldnt result in a Malformed Request Reply
        [Test, Order(2)]
        public void UpdateDeck_CorrectSize()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Payload = "\"[\\\"aa9999a0-734c-49c6-8f4a-651864b14e62\\\", \\\"d6e9c720-9b5a-40c7-a6b2-bc34752e3463\\\", \\\"d60e23cf-2238-4d49-844f-c7589ee5342e\\\", \\\"845f0dc7-37d0-426e-994e-43fc3ac83c08\\\"]\"";
            UserToken userToken = new UserToken();
            CardCollection cardCollection = new CardCollection();
            cardCollection.UpdateDeck(mockEventArgs.Object, userToken);
            mockEventArgs.Verify(e => e.Reply(400, "Malformed Request to update Decks."), Times.Never);
        }
    }
}
