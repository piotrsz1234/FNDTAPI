using System.IO.Compression;
using FDNTAPI.DataModels.Calendar;
using FDNTAPI.DataModels.Posts;
using FDNTAPI.DataModels.TaskLists;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace FDNTAPI {
	public class Startup {
		public Startup (IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices (IServiceCollection services) {
			services.AddControllers ();
			string[] domainsForCors = Configuration.GetValue<string[]>("CORS Domains");
			services.AddCors(x => x.AddDefaultPolicy(builder => {
				builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
			}));
			AddingMongoDbToDependencyInjection (services);
		}

		private void AddingMongoDbToDependencyInjection(IServiceCollection services) {
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
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment ()) {
				app.UseDeveloperExceptionPage ();
			}

			app.UseHttpsRedirection ();
			app.UseRouting ();

			app.UseAuthorization ();
			app.UseCors();
			app.UseEndpoints (endpoints => {
				endpoints.MapControllers ();
			});
		}
	}
}
