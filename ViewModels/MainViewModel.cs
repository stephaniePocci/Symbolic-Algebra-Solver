using Symbolic_Algebra_Solver.Models;

namespace Symbolic_Algebra_Solver.ViewModels
{
    public class MainViewModel
    {
        public SimpleExpression Expression { get; set; }
        
        public MainViewModel()
        {
            Expression = new SimpleExpression();
        }
    }
}
