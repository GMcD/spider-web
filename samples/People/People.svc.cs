using System;
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

    #region Person Service
    /// <summary>
    /// Person Class is a simple test class for the Person Service.
    /// </summary>
    [DataContract]
    [Description("Gary's Person Service, Single Person Implementation.")]
    [Route("/person/{Id}")]
    [Route("/person/{Id*}")]
    public class Person
    {
        /// <summary>
        /// The IoC container injects the DbFactory from the AppHost
        /// </summary>
        public static IDbConnectionFactory DbFactory { get; set; }

        /// <summary>
        /// The Id is an autoincrement property of the Person Store
        /// </summary>
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// The nullable Parent property of the Person identifies the parent Person
        /// </summary>
        [DataMember]
        [ForeignKey(typeof(Person))]
        public int? Parent { get; set; }
        /// <summary>
        /// The FirstName property must not be emtpy, and is the First Name of the Person
        /// </summary>
        [DataMember]
        public string FirstName { get; set; }
        /// <summary>
        /// The LastName property must not be emtpy, and is the Last Name of the Person
        /// </summary>
        [DataMember]
        public string LastName { get; set; }
        /// <summary>
        /// The nullable DateOfBirth is the date of birth of the Person
        /// </summary>
        [DataMember]
        public DateTime? DateOfBirth { get; set; }
        /// <summary>
        /// The hasChildren property of the Person is generated from the Storage Service by the Aynchronous Tree population
        /// </summary>
        public bool hasChildren { get; set; }

        /// <summary>
        /// The Person constructor currently has 'silly' default values, to allow IValidator&lt;Person&gt; to deny empty values
        /// </summary>
        public Person() { FirstName = "No"; LastName = "Name"; }

        /// <summary>
        /// The Display property currently is a weak 'facade' implementation for testing purposes. Represents a Person 'View'.
        /// </summary>
        public string Display
        {
            get
            {
                return DateOfBirth.HasValue
                    ? String.Format("{0} {1} is {2:0} years old.", FirstName, LastName, DateTime.Now.Subtract(DateOfBirth.Value).Days / 365.2425)
                    : String.Format("{0} {1}", FirstName, LastName);
            }
        }

        public static implicit operator Node(Person p)
        {
            return new Node(p.Id, p.FirstName + " " + p.LastName, "/egg/person/" + p.Id, p.hasChildren);
        }
    }

    public class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            RuleFor(p => p.FirstName).NotEmpty();
            RuleFor(p => p.LastName).NotEmpty();
            RuleFor(p => p.DateOfBirth).LessThan(DateTime.Now).When(p => p.DateOfBirth.HasValue);
        }
    }

    [DataContract]
    public class PersonResponse : IHasResponseStatus
    {
        // This mixes the 'all data' for client and the 'server methods' ideas - fix with facade?
        public PersonResponse(Person person)
        {
            this.Person = person;
            this.Label = person.Display;
            this.ResponseStatus = new ResponseStatus();
        }

        public PersonResponse()
        {
            this.Person = new Person();
            this.ResponseStatus = new ResponseStatus();
        }

        [DataMember]
        public Person Person { get; set; }
        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class PersonService : RestServiceBase<Person>
    {
        /// <summary>
        /// The DbFactory is injected by the Funq IoC container, currently MySQL
        /// </summary>
        public IDbConnectionFactory DbFactory { get; set; }
        /// <summary>
        /// The PersonValidator is injected by the Funq IoC container, current all present in assembly
        /// </summary>
        public IValidator<Person> Validator { get; set; }

        /// <summary>
        /// GET /egg/person/{Id} 
        /// </summary>
        public override object OnGet(Person request)
        {
            Person person = DbFactory.Exec(dbCmd => dbCmd.GetById<Person>(request.Id));
            return new PersonResponse { Person = person, Label = person.Display };
        }

        /// <summary>
        /// POST /egg/person/{newPersonId}
        /// </summary>
        public override object OnPost(Person person)
        {
            var newPersonId = DbFactory.Exec(dbCmd =>
            {
                dbCmd.Insert(person);
                return dbCmd.GetLastInsertId();
            });

            var newPerson = new PersonResponse
            {
                Person = DbFactory.Exec(dbCmd => dbCmd.GetById<Person>(newPersonId)),
            };

            return new HttpResult(newPerson)
            {
                StatusCode = HttpStatusCode.Created,
                Headers = {
					{ HttpHeaders.Location, this.RequestContext.AbsoluteUri.WithTrailingSlash() + newPersonId }
				}
            };
        }

        /// <summary>
        /// PUT /egg/person/{id}
        /// </summary>
        public override object OnPut(Person person)
        {
            ValidationResult result = this.Validator.Validate(person);
            if (!result.IsValid)
                throw result.ToException();

            DbFactory.Exec(dbCmd => dbCmd.Save(person));
            return null;
        }

        /// <summary>
        /// DELETE /egg/person/{Id}
        /// </summary>
        public override object OnDelete(Person person)
        {
            DbFactory.Exec(dbCmd => dbCmd.DeleteById<Person>(person.Id));
            return null;
        }

    }
    #endregion

    #region Persons Service
    [DataContract]
    [Description("Gary's Person Service, Person Collection Implementation.")]
    [RestService("/person")]
    //[RestService("/persons/{Id}")]
    //[RestService("/persons/{Id*}")]
    public class Persons
    {
        [DataMember]
        public int? Id { get; set; }
    }

    [DataContract]
    public class PersonsResponse
    {
        public PersonsResponse(){
            Persons = new List<Person>();
        }
        [DataMember]
        public List<Person> Persons { get; set; }
    }

    public class PersonsService : RestServiceBase<Persons>
    {
        public IDbConnectionFactory DbFactory { get; set; }

        //private bool hasChildren(Person person)
        //{
        //    person.hasChildren = DbFactory.Exec(dbCmd => dbCmd.GetScalar<int>("select count(*) from Person where parent = {0}", person.Id) > 0);
        //    return person.hasChildren;
        //}

        //public List<Person> Children(int? parent)
        //{
        //    return DbFactory.Exec(dbCmd =>
        //        parent.HasValue
        //            ? dbCmd.Select<Person>("parent = {0}", parent)
        //            : dbCmd.Select<Person>("parent is null")
        //    );
        //}

        //public object Execute(Persons request)
        //{
        //    int? parent = request.Id;
        //    List<Person> persons = Children(parent);
        //    return new PersonsResponse { Persons = persons };
        //}
        /// <summary>
        /// Return a collection of Persons
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override object OnGet(Persons request)
        {
            return DbFactory.Exec(dbCmd => new PersonsResponse { Persons = dbCmd.Select<Person>() });
        }
    }
    #endregion

    #region Storage Service
    [RestService("/reset-people")]
    [Description("Resets the database back to the original people.")]
    public class Storage { }
    [DataContract]
    public class StorageResponse { }
    public class StorageService : RestServiceBase<Storage>
    {
        public IDbConnectionFactory DbFactory { get; set; }

        public override object OnGet(Storage request)
        {
            DbFactory.Exec(dbCmd =>
            {
                //Re-Create all table schemas:
                dbCmd.DropTable<FamilyMember>();
                dbCmd.DropTable<Person>();
                dbCmd.DropTable<Family>();
                dbCmd.CreateTable<Family>();
                dbCmd.CreateTable<Person>();
                dbCmd.CreateTable<FamilyMember>();

                using (var trans = dbCmd.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dbCmd.Insert(new Family { Id = 1, Name = "Stikova" });
                    dbCmd.Insert(new Family { Id = 2, Name = "MacDonald" });

                    dbCmd.Insert(new Person { Id = 0, FirstName = "People" });
                    dbCmd.Insert(new Person { Id = 1, Parent = 0, FirstName = "Lenka", LastName = "Stikova", DateOfBirth = DateTime.Parse("27/05/1967") });
                    dbCmd.Insert(new Person { Id = 2, Parent = 0, FirstName = "Gary", LastName = "MacDonald", DateOfBirth = DateTime.Parse("27/05/1967") });
                    dbCmd.Insert(new Person { Id = 3, Parent = 1, FirstName = "Michaela", LastName = "MacDonald", DateOfBirth = DateTime.Parse("23/07/1985") });
                    dbCmd.Insert(new Person { Id = 4, Parent = 3, FirstName = "Arthur", LastName = "MacDonald", DateOfBirth = DateTime.Parse("22/12/2010") });
                    dbCmd.Insert(new FamilyMember { FamilyId = 1, PersonId = 1 });
                    dbCmd.Insert(new FamilyMember { FamilyId = 1, PersonId = 3 });
                    dbCmd.Insert(new FamilyMember { FamilyId = 2, PersonId = 2 });
                    dbCmd.Insert(new FamilyMember { FamilyId = 2, PersonId = 3 });
                    dbCmd.Insert(new FamilyMember { FamilyId = 2, PersonId = 4 });
                    trans.Commit();
                }
            });
            return new StorageResponse();
        }
    }
    #endregion

    #region Nodes Service
    [DataContract]
    public class Node
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public string Link { get; set; }
        [DataMember]
        public bool hasChildren { get; set; }
        public Node(int id, string label, string link, bool children)
        {
            Id = id.ToString();
            Label = label;
            Link = link;
            hasChildren = children;
        }
    }
    
    [DataContract]
    [Description("Nodes Service.")]
    [RestService("/nodes")] 
    [RestService("/nodes/{Id}")]
    [RestService("/nodes/{Id*}")]
    public class Nodes
    {
        [DataMember]
        public int? Id { get; set; }
    }

    /// Define your Web Service response (i.e. Response DTO)
    [DataContract]
    public class NodesResponse
    {
        [DataMember]
        public List<Node> Nodes { get; set; }
    }

    public class NodesService : IService<Nodes>
    {
        public IDbConnectionFactory DbFactory { get; set; }

        private bool hasChildren(Person person)
        {
            person.hasChildren = DbFactory.Exec(dbCmd => dbCmd.GetScalar<int>("select count(*) from Person where parent = {0}", person.Id) > 0);
            return person.hasChildren;
        }

        public List<Person> Children(int? parent)
        {
            return DbFactory.Exec(dbCmd =>
                parent.HasValue
                    ? dbCmd.Select<Person>("parent = {0}", parent)
                    : dbCmd.Select<Person>("parent is null")
            );
        }

        public object Execute(Nodes request)
        {
            int? parent = request.Id;
            List<Person> people = Children(parent);
            people.ForEach(p => hasChildren(p));
            List<Node> children = people.ConvertAll(i => (Node)i);
            return new NodesResponse { Nodes = children };
        }
    }
    #endregion

    #region Unused Family Stuff
    public class Family
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class FamilyMember
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Family))]      //Creates Foreign Key
        public int FamilyId { get; set; }
        [ForeignKey(typeof(Person), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public int PersonId { get; set; }
    }
    #endregion

}
