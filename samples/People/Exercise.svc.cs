using System;
using System.Globalization;
using System.Net;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;
using ServiceStack.FluentValidation;
using ServiceStack.FluentValidation.Results;

namespace Rum.People
{

    #region Exercise Service
    /// <summary>
    /// Exercise Class is a simple test class for the Exercise Service.
    /// </summary>
    [DataContract]
    [Description("Gary's Exercise Service.")]
    [Route("/exercise/{id}")]
    public class Exercise : IEquatable<Exercise>
    {
        /// <summary>
        /// The IoC container injects the DbFactory from the AppHost
        /// </summary>
        public static IDbConnectionFactory DbFactory { get; set; }

        /// <summary>
        /// The id is an autoincrement property of the Exercise Store, (currently) utilised in exercise.html
        /// </summary>
        [AutoIncrement]
        [DataMember]
        public int? id { get; set; }
        /// <summary>
        /// The Date property of the Exercise identifies the day
        /// </summary>
        [DataMember]
        public DateTime date { get; set; }
        /// <summary>
        /// The type property is one of Bike, Ride, Swim, Walk as (currently) enumerated in exercise.html
        /// </summary>
        [DataMember]
        public string type { get; set; }
        /// <summary>
        /// The distance field of the exercise is also freeform text, miles or whatever
        /// </summary>
        [DataMember]
        public string distance { get; set; }
        /// <summary>
        /// The duration property of the Exercise will be generated from the end time?
        /// </summary>
        [DataMember]
        public string duration { get; set; }
        /// <summary>
        /// The comments field of the exercise is freeform text
        /// </summary>
        [DataMember]
        public string comments { get; set; }

        // IEquatable Interface
        public bool Equals(Exercise o)
        {
            // Dates are tricky!
            TimeSpan ts = o.date - date;
            return o.id == id && o.type == type && o.duration == duration && o.comments == comments && o.distance == distance ;
        }
    }

    [DataContract]
    public class ExerciseResponse : IHasResponseStatus
    {
        /// <summary>
        /// This allows the new method (OnPut) to return a list, which is strictly not necessary...
        /// The method should just redirect to the new element, via Location Header 
        /// </summary>
        public ExerciseResponse()
        {
            this.Exercises = new List<Exercise>();
            this.ResponseStatus = new ResponseStatus();
        }

        [DataMember]
        public List<Exercise> Exercises { get; set; }
        [DataMember]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ExerciseService : RestServiceBase<Exercise>
    {
        /// <summary>
        /// The DbFactory is injected by the Funq IoC container, currently MySQL
        /// </summary>
        public IDbConnectionFactory DbFactory { get; set; }

        /// <summary>
        /// GET /egg/exercise/{id} to return single exercise, /egg/exercise to return collection
        /// </summary>
        public override object OnGet(Exercise request)
        {
            if (request.id.HasValue)
                return DbFactory.Run(dbCmd => dbCmd.GetById<Exercise>(request.id.Value));

            return DbFactory.Run(dbCmd => dbCmd.Select<Exercise>() );
        }

        /// <summary>
        /// POST /egg/exercise which redirects to /egg/exercise/{newExerciseId}
        /// </summary>
        public override object OnPost(Exercise exercise)
        {
            var newExerciseId = DbFactory.Run(dbCmd =>
            {
                dbCmd.Insert(exercise);
                return dbCmd.GetLastInsertId();
            });

            return DbFactory.Run(dbCmd => dbCmd.GetById<Exercise>(newExerciseId));

        }

        /// <summary>
        /// PUT /egg/exercise/{id}
        /// </summary>
        public override object OnPut(Exercise exercise)
        {
            DbFactory.Exec(dbCmd => dbCmd.Update(exercise));
            return null;
        }

        /// <summary>
        /// DELETE /egg/exercise/{id}
        /// </summary>
        public override object OnDelete(Exercise exercise)
        {
            DbFactory.Run(dbCmd => dbCmd.DeleteById<Exercise>(exercise.id));
            return null;
        }

    }
    #endregion

}
