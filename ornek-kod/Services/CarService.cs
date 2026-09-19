using Dapper;
using Dapper.Contrib.Extensions;
using VincYonetim.Data;
using VincYonetim.Data.DataModel;

namespace VincYonetim.Services
{
    public interface ICarService
    {
        List<Cars> GetCars();
        Cars? GetCar(int id);
        bool AddCar(Cars model);
        bool UpdateCar(Cars model);
        bool DeleteCar(int id);
        bool IsExists(string plateNumber);
    }

    // Dapper ile veri erişimi. Tüm sorgular parametreli (SQL injection'a kapalı).
    public class CarService : BaseService, ICarService
    {
        public CarService(IDBConnector dbConnector) : base(dbConnector) { }

        public List<Cars> GetCars() =>
            Connection.Query<Cars>("SELECT * FROM cars").ToList();

        public Cars? GetCar(int id) =>
            Connection.QueryFirstOrDefault<Cars>("SELECT * FROM cars WHERE id = @id", new { id });

        public bool IsExists(string plateNumber) =>
            Connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM cars WHERE platenumber = @plateNumber", new { plateNumber }) > 0;

        public bool AddCar(Cars model) => Connection.Insert(model) > 0;

        public bool UpdateCar(Cars model) => Connection.Update(model);

        public bool DeleteCar(int id)
        {
            var car = GetCar(id);
            return car != null && Connection.Delete(car);
        }
    }
}
