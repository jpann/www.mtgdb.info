using NUnit.Framework;
using System;
using MtgDb.Info;
using SuperSimple.Auth;
using MtgDb.Info.Driver;

namespace Test_MtgDb.Info
{
    [TestFixture ()]
    public class Test
    {
        private SuperSimpleAuth ssa;
        private IRepository repository;
        private Planeswalker mtgdbUser; 
        private Db mtgdb; 
        private Admin admin; 

        [SetUp()]
        public void Init()
        {
            ssa =  new SuperSimpleAuth ("testing_mtgdb.info", 
                "ae132e62-570f-4ffb-87cc-b9c087b09dfb");

            mtgdb = new Db("http://127.0.0.1:8082");

            repository =  new MongoRepository ("mongodb://localhost", mtgdb, ssa);
            admin = new Admin("http://127.0.0.1:8082");

            //Super simple auth can't delete from here. 
            try
            {
                repository.AddPlaneswalker ("mtgdb_tester", 
                    "test123", "mtgdb_tester@test.com");
            }
            catch(Exception e)
            {
                System.Console.WriteLine (e.Message);
            }

            User ssaUser = ssa.Authenticate ("mtgdb_tester", 
                "test123", "127.0.0.1");

            mtgdbUser = new Planeswalker 
            {
                UserName = ssaUser.UserName,
                AuthToken = ssaUser.AuthToken,
                Email = ssaUser.Email,
                Id = ssaUser.Id,
                Claims = ssaUser.Claims,
                Roles = ssaUser.Roles,
                Profile = repository.GetProfile (ssaUser.Id)
            };
        }

        [Test()]
        public void Add_new_card()
        {
            NewCard card = new NewCard()
            {
                UserId = Guid.NewGuid(),
                Name = "Test card",
                Description = "Test card",
                Flavor = "",
                Type = "Creature",
                SubType = "Test"
            };

            Guid id = repository.AddCard(card);
            Assert.NotNull(id);
        }

        [Test()]
        public void Get_new_cards()
        {
            NewCard card = new NewCard()
            {
                UserId = Guid.NewGuid(),
                Name = "Test card 2",
                Description = "Test card",
                Flavor = "",
                Type = "Creature",
                SubType = "Test"
            };

            repository.AddCard(card);

            NewCard [] cards = repository.GetNewCards();
            Assert.Greater(cards.Length, 0);
        }


        [Test()]
        public void Get_new_card()
        {
            NewCard card = new NewCard()
            {
                UserId = Guid.NewGuid(),
                Name = "Test card 1",
                Description = "Test card",
                Flavor = "",
                Type = "Creature",
                SubType = "Test"
            };

            Guid id = repository.AddCard(card);

            card = repository.GetCard(id);
            Assert.AreEqual(card.Id, id);
        }

        [Test()]
        public void Add_new_set()
        {
            NewSet set = new NewSet()
            {
                UserId = Guid.NewGuid(),
                Name = "Test set",
                Description = "Test set",
                Block = "Test",
                Type = "Test",
                BasicLand = 0, 
                Rare = 0,
                MythicRare = 0, 
                Uncommon = 0, 
                Common = 0
            };

            Guid id = repository.AddSet(set);
            Assert.NotNull(id);
        }

        [Test()]
        public void Get_new_set()
        {
            NewSet set = new NewSet()
            {
                UserId = Guid.NewGuid(),
                SetId = "TST",
                Name = "Test set",
                Description = "Test set",
                Block = "Test",
                Type = "Test",
                BasicLand = 0, 
                Rare = 0,
                MythicRare = 0, 
                Uncommon = 0, 
                Common = 0
            };

            Guid id = repository.AddSet(set);
            set = repository.GetSet(id);

            Assert.AreEqual(set.Id, id);
        }

        [Test()]
        public void Get_new_sets()
        {
            NewSet set = new NewSet()
            {
                UserId = Guid.NewGuid(),
                SetId = "TST2",
                Name = "Test set",
                Description = "Test set",
                Block = "Test",
                Type = "Test",
                BasicLand = 0, 
                Rare = 0,
                MythicRare = 0, 
                Uncommon = 0, 
                Common = 0
            };

            NewSet [] sets = repository.GetNewSets();
            Assert.Greater(sets.Length, 0);
        }

        [Test()]
        public void Update_new_set_staus()
        {
            NewSet set = new NewSet()
            {
                UserId = Guid.NewGuid(),
                SetId = "TST3",
                Name = "Test set",
                Description = "Test set",
                Block = "Test",
                Type = "Test",
                BasicLand = 0, 
                Rare = 0,
                MythicRare = 0, 
                Uncommon = 0, 
                Common = 0
            };

            Guid id = repository.AddSet(set);
            set = repository.UpdateNewSetStatus(id,"Declined");
            Assert.AreEqual(set.Status, "Declined");
        }
            
        [Test()]
        public void Update_change_status()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(2);

            change = CardChange.MapCard(card);
            change.Comment = "test";
            change.Description = "lucky";
            change.UserId = Guid.NewGuid();

            Guid id = repository.AddCardChangeRequest(change);

            string value = change.GetFieldValue("description");

            admin.UpdateCardField(mtgdbUser.AuthToken, change.Mvid, "description", value);
            repository.UpdateCardChangeStatus(id, "Accepted", "description");

            change = repository.GetCardChangeRequest(id);

            Assert.AreEqual(change.Status, "Accepted" );
            Assert.Greater(change.ChangesCommitted.Length, 0);
        }

        [Test()]
        public void Update_setchange_status()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(2);

            change = CardChange.MapCard(card);
            change.Comment = "test";
            change.Description = "lucky";
            change.UserId = Guid.NewGuid();

            Guid id = repository.AddCardChangeRequest(change);

            string value = change.GetFieldValue("description");

            admin.UpdateCardField(mtgdbUser.AuthToken, change.Mvid, "description", value);
            repository.UpdateCardChangeStatus(id, "Accepted", "description");

            change = repository.GetCardChangeRequest(id);

            Assert.AreEqual(change.Status, "Accepted" );
            Assert.Greater(change.ChangesCommitted.Length, 0);
        }

            
        [Test()]
        public void Make_change_to_field()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(2);

            change = CardChange.MapCard(card);
            change.Comment = "test";
            change.Description = Guid.NewGuid().ToString();
            change.UserId = Guid.NewGuid();

            Guid id = repository.AddCardChangeRequest(change);

            string value = change.GetFieldValue("description");

            admin.UpdateCardField(mtgdbUser.AuthToken, change.Mvid, "description",value);

            card = mtgdb.GetCard(2);

            Assert.AreEqual(card.Description, change.Description );
        }

        [Test()]
        public void Get_change_value()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(2);

            change = CardChange.MapCard(card);
            change.Comment = "test";
            change.Description = "test";
            change.UserId = Guid.NewGuid();

            Guid id = repository.AddCardChangeRequest(change);

            string value = change.GetFieldValue("description");

            Assert.AreEqual(value, "test" );
        }

        [Test()]
        public void Get_change_requests()
        {
            CardChange [] changes = repository.GetChangeRequests();

            Assert.Greater(changes.Length, 0);
        }
            
        [Test()]
        public void Get_card_change()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(2);

            change = CardChange.MapCard(card);
            change.Description = "1";
            change.Comment = "test";
            change.UserId = Guid.NewGuid();

            Guid id = repository.AddCardChangeRequest(change);

            change = repository.GetCardChangeRequest(id);

            Assert.AreEqual(id, change.Id );
        }


        [Test()]
        public void Get_card_changes()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(2);

            change = CardChange.MapCard(card);
            change.Comment = "test";
            change.Description = "1";
            //change.UserId = Guid.NewGuid();

            repository.AddCardChangeRequest(change);

            CardChange[] changes = repository.GetCardChangeRequests(2);

            Assert.Greater(changes.Length, 0);
        }

        [Test()]
        public void Add_card_change()
        {
            CardChange change = new CardChange();

            Card card = mtgdb.GetCard(1);

            change = CardChange.MapCard(card);
            change.Description = "description change";
            change.Comment = "test";
            change.UserId = Guid.NewGuid();

            Guid newId = repository.AddCardChangeRequest(change);

            Assert.AreNotEqual(Guid.Empty,newId);
        }

        [Test()]
        public void Add_set_change()
        {
            SetChange change = new SetChange();

            CardSet set = mtgdb.GetSet("THS");

            change = SetChange.MapSet(set);
            change.Description = "description change";
            change.Comment = "test";
            change.UserId = Guid.NewGuid();

            Guid newId = repository.AddCardSetChangeRequest(change);

            Assert.AreNotEqual(Guid.Empty,newId);
        }

        [Test()]
        public void Get_set_change()
        {
            SetChange change = new SetChange();

            CardSet set = mtgdb.GetSet("THS");

            change = SetChange.MapSet(set);
            change.Description = "1";
            change.Comment = "test";
            change.UserId = Guid.NewGuid();

            Guid id = repository.AddCardSetChangeRequest(change);

            change = repository.GetCardSetChangeRequest(id);

            Assert.AreEqual(id, change.Id );
        }

        [Test()]
        public void Get_cardset_changes()
        {
            SetChange change = new SetChange();

            CardSet set = mtgdb.GetSet("THS");

            change = SetChange.MapSet(set);
            change.Comment = "test";
            change.Description = "1";

            repository.AddCardSetChangeRequest(change);

            SetChange[] changes = repository.GetCardSetChangeRequests("THS");

            Assert.Greater(changes.Length, 0);
        }

        [Test()]
        public void Get_set_change_requests()
        {
            SetChange [] changes = repository.GetSetChangeRequests();

            Assert.Greater(changes.Length, 0);
        }
            
        [Test ()]
        public void Get_UserCards_by_setId ()
        {
            repository.AddUserCard(mtgdbUser.Id, 1, 1);
            repository.AddUserCard(mtgdbUser.Id, 2, 3);

            UserCard [] userCards = repository.GetUserCards(mtgdbUser.Id, "LEA");

            Assert.AreEqual (2, userCards.Length);
        }

        [Test ()]
        public void Get_UserCards ()
        {
            repository.AddUserCard(mtgdbUser.Id, 1, 1);
            repository.AddUserCard(mtgdbUser.Id, 2, 3);

            UserCard [] userCards = repository.GetUserCards(mtgdbUser.Id,new int[]{1, 2});

            Assert.AreEqual (2, userCards.Length);
        }

        [Test ()]
        public void Add_UserCard ()
        {
            UserCard card = repository.AddUserCard(mtgdbUser.Id, 1, 1);
            Assert.AreEqual (1, card.Amount);
        }

        [Test ()]
        public void Add_Planeswalker ()
        {
            string user = Guid.NewGuid ().ToString ();
            Guid id = repository.AddPlaneswalker (user,"test12345", user + "@test.com");
            Assert.IsNotNull (id);

            //Clean up
            repository.RemovePlaneswalker (id);
        }

        [Test ()]
        public void Remove_Planeswalker ()
        {
            string user = Guid.NewGuid ().ToString ();
            Guid id = repository.AddPlaneswalker (user,"test12345", user + "@test.com");
            Assert.IsNotNull (id);
            //Clean up
            repository.RemovePlaneswalker (id);

            Assert.IsNull(repository.GetProfile (id));
        }

        [Test ()]
        public void Get_Profile ()
        {
            Profile profile = repository.GetProfile (mtgdbUser.Id);
            Assert.AreEqual (mtgdbUser.Id, profile.Id);
        }

        [Test ()]
        public void Update_Planeswalker ()
        {
            mtgdbUser.Profile.Email = "change@email.com";
            mtgdbUser = repository.UpdatePlaneswalker(mtgdbUser);
            Assert.AreEqual (mtgdbUser.Email, "change@email.com");

            mtgdbUser.Profile.Email = "mtgdb_tester@test.com";
            mtgdbUser = repository.UpdatePlaneswalker(mtgdbUser);
            Assert.AreEqual (mtgdbUser.Email, "mtgdb_tester@test.com");

        }
    }
}

