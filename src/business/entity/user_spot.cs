namespace business.entity
{
    public abstract class UserSpot
    {
        public abstract bool IsOfType(int type);
        public abstract string Description { get; }
        public abstract string Name { get; }
        public abstract int InsertUser(User user);
        public abstract void RemoveUser(User user);
        public abstract bool IsUserIn(string userName);
        public abstract int UserCount { get; }
    }
}
