using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.ViewModel;

namespace ComponentBalanceSqlv2.Model.ParametrsBuilder
{
    public class MainParametrsBuilder : IParametrsBuilder
    {
        private readonly ParametrsViewModel _parametrsViewModel;

        public MainParametrsBuilder(MonthYear monthYear)
        {
            _parametrsViewModel = new ParametrsViewModel(monthYear);
        }

        public ParametrsViewModel GetParametrsViewModel()
        {
            return _parametrsViewModel;
        }

        public void SetMessage(string message)
        {
            _parametrsViewModel.Message = message;
        }

        public void SetIsMonthYear(bool flag)
        {
            _parametrsViewModel.IsMonthYear = flag;
        }

        public void SetIsWorkGuild(bool flag)
        {
            _parametrsViewModel.IsWorkGuild = flag;
        }

        public void SetIsWorkGuildOrAll(bool flag)
        {
            _parametrsViewModel.IsWorkGuildOrAll = flag;
        }

        public void SetIsTmcType(bool flag)
        {
            _parametrsViewModel.IsTmcType = flag;
        }

        public void SetIsProduct(bool flag)
        {
            _parametrsViewModel.IsProduct = flag;
        }

        public void SetIsPartRelease(bool flag)
        {
            _parametrsViewModel.IsPartRelease = flag;
        }
    }
}