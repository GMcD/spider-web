jQuery(function ($) {

    var Person = Backbone.Model.extend({
        idAttribute: 'Id',
        url: function () {
            return '/egg/person/' + this.get("Id");
        }
    });

    var Persons = Backbone.Collection.extend({
        model: Person,
        url: '/egg/person',
        parse: function (data) {
            return data.Persons;
        } /*,
        initialize: function () {
            this.bind("add", function (model) {
                alert("hey");
                view.render(model);
            });
        } */
    });

    // Views
    var PersonDetailsView = Backbone.View.extend({
        tagName: 'ul',
        template: _.template($('#personDetails-template').html()),
        initialize: function () {
            this.model.bind("change", this.render, this);
        },
        render: function (eventName) {
            var person = this.model.toJSON();
            var html = this.template(person);
            $(this.el).html(html);
            return this;
        },
        events: {
            "change input": "change",
            "click span#save": "savePerson" /*,
            "click span#delete": "deletePerson" */
        },
        change: function (event) {
            var target = event.target;
            alert('changing ' + target.id + ' from: ' + target.defaultValue + ' to: ' + target.value);
            // You could change your model on the spot, like this - seems to fire close:
            //var change = {};
            //change[target.id] = target.value;
            //this.model.set(change);
        },
        savePerson: function () {
            alert($('#FirstName').val());
            this.model.set({
                FirstName: $('#FirstName').val(),
                LastName: $('#LastName').val(),
                DateOfBirth: $('#DateOfBirth').val()
            });
            if (this.model.isNew()) {
                app.personsList.create(this.model);
            } else {
                this.model.save();
            }
            return false;
        }, /*
        deletePerson: function () {
            this.model.destroy({
                success: function () {
                    alert('Person deleted successfully');
                    window.history.back();
                }
            });
            return false;
        }, */
        close: function () {
            $(this.el).unbind();
            $(this.el).empty();
        }
    });

    var PersonView = Backbone.View.extend({
        tagName: "li",
        template: _.template($('#persons-template').html()),
        initialize: function () {
            this.model.bind("change", this.render, this);
            this.model.bind("destroy", this.close, this);
        },
        events: {
            "click span#delete": "deletePerson",
            "click span#edit": "editPerson",
            "click span#save": "savePerson",
            "click span#up": "close"
        },
        render: function (eventName) {
            var person = this.model.toJSON();
            var html = this.template(person);
            $(this.el).html(html);
            return this;
        },
        editPerson: function () {
            if (this.detailsView == null) {
                this.detailsView = new PersonDetailsView({ model: this.model });
                $(this.el).append(this.detailsView.render().el);
            }
            return false;
        },
        savePerson: function () {
            alert('Saving person...');
            if (this.detailsView != null) {
                this.model.save({
                    success: function () {
                        alert('Person saved successfully');
                    },
                    error: function () {
                        alert('Person not saved...');
                    }
                });
                this.detailsView.close();
                this.detailsView = null;
            }
            return false;
        },
        deletePerson: function () {
            this.model.destroy({
                success: function () {
                    alert('Person deleted successfully');
                    window.history.back();
                }
            });
            return false;
        },
        close: function () {
            $(this.el).unbind();
            $(this.el).remove();
            window.history.back();
            app.personView = null;
        }
    });

    var PersonsView = Backbone.View.extend({
        tagName: 'ul',
        initialize: function () {
            this.model.bind("reset", this.render, this);
            var self = this;
            this.model.bind("add", function (person) {
                $(self.el).append(new PersonView({ model: person }).render().el);
            });
        },
        render: function (eventName) {
            _.each(this.model.models, function (person) {
                $(this.el).append(new PersonView({ model: person }).render().el);
            }, this);
            return this;
        }
    });
    var HeaderView = Backbone.View.extend({
        tagName: 'ul',
        template: _.template($('#person-header').html()),
        initialize: function () {
            this.render();
        },
        render: function (eventName) {
            $(this.el).html(this.template());
            return this;
        },
        events: {
            "click span#new": "newPerson",
            "click span#refresh": "reload"
        },
        newPerson: function (event) {
            if (app.personView) app.personView.close();
            app.personView = new PersonView({ model: new Person({ "Id": 5, "Parent": 4, "FirstName": "Abc", "LastName": "Def", DateOfBirth: null }) });
            $('div#bb>ul').append(app.personView.render().el);
            app.personView.editPerson();
            return false;
        },
        reload: function (event) {
            app.list();
        }
    });

    // Router
    var AppRouter = Backbone.Router.extend({
        routes: {
            "": "list",
            "egg/person/:id": "personDetails"
        },
        initialize: function () {
            $('#header').html(new HeaderView().render().el);
        },
        list: function () {
            this.persons = new Persons();
            this.personsView = new PersonsView({ model: this.persons });
            this.persons.fetch();
            $('div#bb').html(this.personsView.render().el);
        },
        personDetails: function (id) {
            if (this.personView) {
                alert("Closing!");
                this.personView.close();
                this.personView = null;
            } else {
                alert("Getting New One!");
                this.person = this.persons.get(id);
                this.personView = new PersonView({ model: this.person });
                $('div#bb>ul').html(this.personView.render().el);
            }
        }
    });

    var app = new AppRouter();
    Backbone.history.start();
});
