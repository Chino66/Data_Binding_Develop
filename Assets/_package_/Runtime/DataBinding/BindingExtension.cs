namespace DataBinding
{
    public static class BindingExtension
    {
        public static Binding GetBinding(this object instance, bool bindingIfNull = true)
        {
            var binding = BindingCollection.GetBinding(instance);
            if (binding != null)
            {
                return binding;
            }

            if (bindingIfNull == false)
            {
                return null;
            }
            else
            {
                binding = new Binding(instance);
            }

            return binding;
        }
    }
}