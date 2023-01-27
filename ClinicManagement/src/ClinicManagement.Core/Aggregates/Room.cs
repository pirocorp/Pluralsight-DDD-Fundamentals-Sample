using PluralsightDdd.SharedKernel;
using PluralsightDdd.SharedKernel.Interfaces;

namespace ClinicManagement.Core.Aggregates
{
  public class Room : BaseEntity<int>, IAggregateRoot
  {
    private Room()
    {
      
    }

    public Room(int id, string name)
    {
      Id = id;
      Name = name;
    }

    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}
