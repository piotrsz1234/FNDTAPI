using FNDTAPI.DataModels.Calendar;
using FNDTAPI.DataModels.Posts;
using FNDTAPI.DataModels.TaskLists;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace FNDTAPI {
	public class Startup {
		public Startup (IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices (IServiceCollection services) {
			services.AddControllers ();
			AddingMongoDBToDependencyInjection (services);
		}

		private void AddingMongoDBToDependencyInjection(IServiceCollection services) {
			IMongoClient mongoClient = new MongoClient (Configuration.GetValue<string> ("MongoDBConnectionString"));
			services.AddSingleton (mongoClient);
			IMongoDatabase database = mongoClient.GetDatabase (Configuration.GetValue<string> ("MongoDBDatabaseName"));
			services.AddSingleton (database.GetCollection<CalendarEvent> (Configuration.GetValue<string> ("CalendarEventCollectionName")));
			services.AddSingleton (database.GetCollection<ParticipationRegistration> (Configuration.GetValue<string> ("ParticipationRegistrationCollectionName")));
			services.AddSingleton (database.GetCollection<TaskList> (Configuration.GetValue<string> ("TaskListCollectionName")));
			services.AddSingleton (database.GetCollection<Task> (Configuration.GetValue<string> ("TaskCollectionName")));
			services.AddSingleton (database.GetCollection<PersonTaskCompletionDeclaration> (Configuration.GetValue<string> ("PersonTaskCompletionDeclarationCollectionName")));
			services.AddSingleton (database.GetCollection<CalendarEventCategory> (Configuration.GetValue<string> ("CalendarEventCategoryCollectionName")));
			services.AddSingleton (database.GetCollection<Post> (Configuration.GetValue<string> ("PostCollectionName")));
			services.AddSingleton (database.GetCollection<OldVersionOfPost> (Configuration.GetValue<string> ("OldVersionOfPostCollectionName")));
			services.AddSingleton (database.GetCollection<Attachment> (Configuration.GetValue<string> ("AttachmentCollectionName")));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment ()) {
				app.UseDeveloperExceptionPage ();
			}

			app.UseHttpsRedirection ();
			
			app.UseRouting ();

			app.UseAuthorization ();

			app.UseEndpoints (endpoints => {
				endpoints.MapControllers ();
			});
		}
	}
}
