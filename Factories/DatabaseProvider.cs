using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Factories
{
    public class DatabaseProvider
    {
        private readonly Task<Database> _dbTask;

        public DatabaseProvider()
        {
            //Initializes the database async, in case of many overlays?
            _dbTask = Database.BuildDatabaseAsync();
        }

        public Task<Database> GetDatabaseAsync() => _dbTask;
    }
}
