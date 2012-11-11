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
    [Route("/exercise/{Id}")]
    [Route("/exercise/{Id*}")]
    public class Exercise
    {
        /// <summary>
        /// The IoC container injects the DbFactory from the AppHost
        /// </summary>
        public static IDbConnectionFactory DbFactory { get; set; }

        /// <summary>
        /// The Id is an autoincrement property of the Exercise Store
        /// </summary>
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// The Date property of the Exercise identifies the time
        /// </summary>
        [DataMember]
        public DateTime date { get; set; }
        /// <summary>
        /// The Type property must not be emtpy, and is the First Name of the Person
        /// </summary>
        [DataMember]
        public string type { get; set; }
        /// <summary>
        /// The LastName property must not be emtpy, and is the Last Name of the Person
        /// </summary>
        [DataMember]
        public string distance { get; set; }
        /// <summary>
        /// The nullable DateOfBirth is the date of birth of the Person
        /// </summary>
        [DataMember]
        public string comments { get; set; }
        /// <summary>
        /// The minutes property of the Exercise will generated the end time?
        /// </summary>
        public int minutes { get; set; }

    }

    [DataContract]
    public class ExerciseResponse : IHasResponseStatus
    {
        // This mixes the 'all data' for client and the 'server methods' ideas - fix with facade?
        public ExerciseResponse(Exercise exercise)
        {
            this.Exercise = exercise;
            this.ResponseStatus = new ResponseStatus();
        }

        public ExerciseResponse()
        {
            this.Exercise = new Exercise();
            this.ResponseStatus = new ResponseStatus();
        }

        [DataMember]
        public Exercise Exercise { get; set; }
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
        /// GET /egg/exercise/{Id} 
        /// </summary>
        public override object OnGet(Exercise request)
        {
            return DbFactory.Run(dbCmd => dbCmd.GetById<Exercise>(request.Id));
        //    Exercise exercise = DbFactory.Run(dbCmd => dbCmd.GetById<Exercise>(request.Id));
        //    return new ExerciseResponse { Exercise = exercise, ResponseStatus = new ResponseStatus() };
        }

        /// <summary>
        /// POST /egg/exercise/{newExerciseId}
        /// </summary>
        public override object OnPost(Exercise exercise)
        {
            var newPersonId = DbFactory.Run(dbCmd =>
            {
                dbCmd.Insert(exercise);
                return dbCmd.GetLastInsertId();
            });

            var newExercise = new ExerciseResponse
            {
                Exercise = DbFactory.Run(dbCmd => dbCmd.GetById<Exercise>(newPersonId)),
            };

            return new HttpResult(newExercise)
            {
                StatusCode = HttpStatusCode.Created,
                Headers = {
					{ HttpHeaders.Location, this.RequestContext.AbsoluteUri.WithTrailingSlash() + newPersonId }
				}
            };
        }

        /// <summary>
        /// PUT /egg/exercise/{id}
        /// </summary>
        public override object OnPut(Exercise exercise)
        {
            DbFactory.Run(dbCmd => dbCmd.Save(exercise));
            return null;
        }

        /// <summary>
        /// DELETE /egg/exercise/{Id}
        /// </summary>
        public override object OnDelete(Exercise exercise)
        {
            DbFactory.Run(dbCmd => dbCmd.DeleteById<Exercise>(exercise.Id));
            return null;
        }

    }
    #endregion

    #region Exercises Service
    [DataContract]
    [Description("Gary's Exercise Service, Exercises Implementation.")]
    [RestService("/exercise")]
    public class Exercises
    {
        [DataMember]
        public int? Id { get; set; }
    }

    [DataContract]
    public class ExercisesResponse
    {
        public ExercisesResponse(){
            Exercises = new List<Exercise>();
        }
        [DataMember]
        public List<Exercise> Exercises { get; set; }
    }

    /// <summary>
    /// Returns a Collection of Exercises
    /// </summary>
    public class ExercisesService : RestServiceBase<Exercises>
    {
        public IDbConnectionFactory DbFactory { get; set; }

        /// <summary>
        /// Return a collection of Exercises
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override object OnGet(Exercises request)
        {
            return DbFactory.Run(dbCmd => new ExercisesResponse { Exercises = dbCmd.Select<Exercise>() });
        }
    }
    #endregion

}
