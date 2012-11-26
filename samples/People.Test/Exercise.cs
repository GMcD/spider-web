using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rum.People;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceClient.Web;

namespace People.Test
{
    [TestFixture]
    public class ExerciseTest
    {
        private JsonServiceClient jsonRestClient;
        [TestFixtureSetUp]
        public void setUp()
        {
            jsonRestClient = new JsonServiceClient("https://devel.projectscapa.com/egg/");
        }

        [Test]
        public void loadExercises()
        {
            var res = jsonRestClient.Get<List<Storage>>("/reset-people");
        }
        [Test]
        public void getExercises()
        {
            List<Exercise> all = jsonRestClient.Get<List<Exercise>>("/exercise");
            Assert.AreEqual(all.Count, 3);
        }
        [Test]
        public void putExercise()
        {
            Exercise exercise = new Exercise { date = DateTime.Now, mode = 5, duration = "5 mins", distance = "10km", comments = "putExercise Test" };
            Exercise newExercise = jsonRestClient.Post<Exercise>("/exercise", exercise);
            Assert.AreEqual(newExercise.id, 4);
            exercise.id = 4;
            Assert.IsTrue(exercise.Equals(newExercise) == true, "putExercise returned different Exercise than sent!");
        }
    }
}
