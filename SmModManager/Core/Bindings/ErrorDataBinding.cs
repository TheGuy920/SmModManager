namespace SmModManager.Core.Bindings
{

    public class ErrorDataBinding
    {

        public string Name { get; private set; }
        public string Function { get; private set; }
        public bool BoxIsChecked { get; set; }

        public static ErrorDataBinding Create(string Name, string func)
        {
            var binding = new ErrorDataBinding
            {
                Name = Name,
                Function = func
            };
            return binding;
        }

    }

}