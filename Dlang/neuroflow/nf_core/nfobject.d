interface NFObject
{
    @property void* primaryPtr();

    @property void primaryPtr(void* ptr);

    @property void* secondaryPtr();
    
    @property void secondaryPtr(void* ptr);
}