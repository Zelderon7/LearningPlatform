namespace WebApplication1.Models.VMs
{
    public class LessonAndContentVM
    {
        public LessonVM Lesson { get; set; }
        public LessonContentVM LessonContent { get; set; }

        public LessonAndContentVM()
        {
            Lesson = new LessonVM();
            LessonContent = new LessonContentVM();
        }
    }
}
